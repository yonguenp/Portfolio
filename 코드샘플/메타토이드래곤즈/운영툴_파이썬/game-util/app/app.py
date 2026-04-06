import os
import logging
import re # 정규표현식 모듈 추가
import csv
import shutil

from functools import wraps
from flask import Flask, render_template, request, redirect, url_for, session, flash
from sqlalchemy import create_engine, text, exc
from datetime import datetime
import pandas as pd
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from sql import REVENUE_SQL, MAGNITE_SUMMARY_SQL, MAGNITE_DETAIL_SQL, USERS_REVENUE_SQL
from datetime import datetime, timedelta
from zoneinfo import ZoneInfo

DEFAULT_END = datetime.now(ZoneInfo("Asia/Seoul")).strftime("%Y-%m-%d %H:%M:%S")
DEFAULT_START = (datetime.now(ZoneInfo("Asia/Seoul")) - timedelta(days=7)).strftime("%Y-%m-%d %H:%M:%S")
RESERV_MAIL_CSV_ORIGIN_PATH = "/usr/share/nginx/mtdz_live/assets/reserv_mail.csv"
RESERV_MAIL_CSV_PATH = "/app/data/reserv_mail.csv"
RESERV_MAIL_TITLE_CSV_PATH = "/usr/share/nginx/mtdz_live/assets/mail_kor.csv"
RESERV_MAIL_REWARD_CSV_PATH = "/usr/share/nginx/mtdz_live/assets/post_reward.csv"
ITEM_BASE_CSV_PATH = "/usr/share/nginx/mtdz_live/assets/item_base.csv"

# --- 프록시 경로 수정을 위한 미들웨어 (이전과 동일) ---
class ReverseProxied(object):
    def __init__(self, app, script_name=None, scheme=None, server=None):
        self.app = app
        self.script_name = script_name
        self.scheme = scheme
        self.server = server
    def __call__(self, environ, start_response):
        script_name = environ.get('HTTP_X_SCRIPT_NAME', '') or self.script_name
        if script_name:
            environ['SCRIPT_NAME'] = script_name
            path_info = environ['PATH_INFO']
            if path_info.startswith(script_name):
                environ['PATH_INFO'] = path_info[len(script_name):]
        scheme = environ.get('HTTP_X_FORWARDED_PROTO', '') or self.scheme
        if scheme:
            environ['wsgi.url_scheme'] = scheme
        server = environ.get('HTTP_X_FORWARDED_HOST', '') or self.server
        if server:
            environ['HTTP_HOST'] = server
        return self.app(environ, start_response)

app = Flask(__name__)
app.wsgi_app = ReverseProxied(app.wsgi_app, script_name='/util')
app.secret_key = '보안'
logging.basicConfig(level=logging.INFO, format='%(asctime)s [%(levelname)s] %(message)s')

# --- 상수 정의 (이전과 동일) ---
SHEET_ID = '1Pls524KMUlrCoN-3sJlggvyEBsIPPEV0e8XWplqHBXU'
SHEET_RANGE_SHOP = 'shop_sku!A:B'
SHEET_RANGE_REWARD = 'post_reward!A:B' # [추가] post_reward 시트 범위
DATA_DIR = '/app/data'
SKU_CSV_PATH = os.path.join(DATA_DIR, 'sku_map.csv')

# --- DB 설정 (이전과 동일) ---
DB_USER = os.environ.get('DB_USER')
DB_PASS = os.environ.get('DB_PASS')
DB_HOST_RO = os.environ.get('DB_HOST_RO')
DB_HOST_RW = os.environ.get('DB_HOST_RW')
DB_PORT = os.environ.get('DB_PORT')

def get_db_engine(db_name='mtd-game-2'):
    db_uri = f"mysql+pymysql://{DB_USER}:{DB_PASS}@{DB_HOST_RO}:{DB_PORT}/{db_name}"
    return create_engine(db_uri)

def get_db_engine_rw(db_name='mtd-game-2'):
    """쓰기 전용 DB 엔진을 반환합니다."""
    if not DB_HOST_RW:
        logging.error("Write DB Host (DB_HOST_RW) is not configured.")
        return None
    db_uri = f"mysql+pymysql://{DB_USER}:{DB_PASS}@{DB_HOST_RW}:{DB_PORT}/{db_name}"
    return create_engine(db_uri)
    
@app.template_filter('comma')
def comma_format(value):
    try:
        return f"{int(value):,}"
    except (ValueError, TypeError):
        return value
        
# --- Google Sheet 및 SKU 데이터 처리 (이전과 동일) ---
def update_sku_data():
    """
    [수정] shop_sku와 post_reward 시트 데이터를 모두 읽어와 병합 후 CSV로 저장합니다.
    """
    try:
        creds_path = os.environ.get('GOOGLE_APPLICATION_CREDENTIALS')
        if not creds_path or not os.path.exists(creds_path):
            msg = "Google API credentials.json 파일을 찾을 수 없습니다."
            logging.error(msg)
            return False, msg

        creds = service_account.Credentials.from_service_account_file(creds_path, scopes=['https://www.googleapis.com/auth/spreadsheets.readonly'])
        service = build('sheets', 'v4', credentials=creds)
        sheet = service.spreadsheets()

        # batchGet을 사용하여 여러 시트 데이터를 한 번에 요청
        result = sheet.values().batchGet(
            spreadsheetId=SHEET_ID,
            ranges=[SHEET_RANGE_SHOP, SHEET_RANGE_REWARD]
        ).execute()
        
        value_ranges = result.get('valueRanges', [])
        all_values = []
        for value_range in value_ranges:
            values = value_range.get('values', [])
            if values:
                all_values.extend(values)

        if not all_values:
            msg = "Google Sheet의 'shop_sku' 및 'post_reward' 시트에서 데이터를 가져오지 못했습니다."
            logging.warning(msg)
            return False, msg

        df = pd.DataFrame(all_values)
        if df.shape[1] < 2:
            msg = "Google Sheet에 최소 2개의 열(상품명, SKU)이 필요합니다."
            logging.warning(msg)
            return False, msg
        
        df = df.iloc[:, [0, 1]]
        df.columns = ['desc', 'KEY']
        
        # 중복된 KEY가 있을 경우, 마지막에 읽은 값으로 덮어씁니다.
        df.drop_duplicates(subset=['KEY'], keep='last', inplace=True)
        
        os.makedirs(DATA_DIR, exist_ok=True)
        df.to_csv(SKU_CSV_PATH, index=False)
        logging.info(f"SUCCESS: SKU data from both sheets has been updated and saved to {SKU_CSV_PATH}")
        return True, "SKU 정보(shop_sku, post_reward)가 성공적으로 업데이트되었습니다."
    except HttpError as err:
        msg = f"Google API 오류: {err}"
        logging.error(msg)
        return False, msg
    except Exception as e:
        msg = f"SKU 데이터 업데이트 중 예상치 못한 오류 발생: {e}"
        logging.error(msg)
        return False, msg

def load_sku_map():
    if not os.path.exists(SKU_CSV_PATH):
        return {}
    try:
        df = pd.read_csv(SKU_CSV_PATH)
        # 데이터 타입을 문자열로 통일하여 불일치 문제 방지
        df = df.astype(str)
        return pd.Series(df.desc.values, index=df.KEY).to_dict()
    except Exception as e:
        logging.error(f"Error loading SKU map from CSV: {e}")
        return {}

# --- 데이터 조회 로직 ---
def search_payment_history_by_userno(engine, user_no, sku_map):
    if sku_map:
        logging.info(f"SKU map loaded with {len(sku_map)} items. First 5: {dict(list(sku_map.items())[:5])}")
    else:
        logging.warning("SKU map is empty or not loaded.")

    try:
        with engine.connect() as conn:
            payment_query = text("""
                SELECT iaptest.register_time, iaptest.order_id, iapOK.market, iapOK.product_id as sku, iapOK.price
                FROM in_app_purchase_test AS iaptest LEFT JOIN in_app_purchases AS iapOK ON iaptest.order_id = iapOK.order_id
                WHERE iaptest.user_no = :user_no ORDER BY iaptest.register_time DESC
            """)
            results = conn.execute(payment_query, {"user_no": user_no}).fetchall()
            formatted = []
            for row in results:
                is_success = bool(row.sku)
                sku_val = str(row.sku) if row.sku else 'N/A'
                
                product_name = sku_map.get(sku_val, sku_val)

                if product_name != sku_val:
                    logging.info(f"SKU translation SUCCESS: '{sku_val}' -> '{product_name}'")
                else:
                    if sku_val != 'N/A':
                        logging.warning(f"SKU translation FAILED for: '{sku_val}'. Not found in map.")
                
                formatted.append({
                    "register_time": row.register_time.strftime('%Y-%m-%d %H:%M:%S'),
                    "market": {1: 'Google', 2: 'Apple'}.get(row.market, 'N/A'),
                    "order_id": row.order_id,
                    "sku": product_name,
                    "price": row.price or 0,
                    "receipt_status": "완료" if is_success else "대기",
                    "grant_status": "지급 완료" if is_success else "지급 대기",
                    "mail_status": "발송 완료" if is_success else "미발송",
                    "mail_button_enabled": is_success
                })
            return formatted
    except exc.SQLAlchemyError as e:
        logging.error(f"DB ERROR fetching payment history: {e}")
        return None

def find_user_no(conn, search_type, query):
    """닉네임 또는 유저번호로 user_no를 찾는 헬퍼 함수"""
    user_no = None
    if search_type == 'nickname':
        user_base_query = text("SELECT user_no FROM user_base WHERE nick = :nick")
        result = conn.execute(user_base_query, {"nick": query}).fetchone()
        if result:
            user_no = result[0]
            logging.info(f"SUCCESS: Found user_no '{user_no}' for nickname '{query}'.")
        else:
            logging.warning(f"INFO: User with nickname '{query}' not found in user_base.")
    elif search_type == 'user_id' and query.isdigit():
        user_no = int(query)
        logging.info(f"INFO: Using user_no '{user_no}' directly from query.")
    return user_no

def search_mailbox(engine, user_no, sku_map):
    """
    user_no로 메일박스를 조회하는 함수.
    [수정] mail_tag에 포함된 sku를 상품명으로 치환하는 로직 추가.
    """
    logging.info(f"Searching mailbox for user_no '{user_no}'.")
    try:
        with engine.connect() as conn:
            query = text("SELECT send_at, received_at, is_receive, is_delete, mail_tag, idx_no, items FROM mail_box WHERE user_no = :user_no ORDER BY send_at DESC")
            results = conn.execute(query, {"user_no": user_no}).fetchall()
            logging.info(f"Found {len(results)} mail records for user_no '{user_no}'.")
            
           # ##########################################################################
            # ### [수정 시작] 우편함 SKU 'startswith' 매칭 로직 ###
            # ##########################################################################
            #
            # 구매내역과 동일하게, 정확한 매칭을 위해 SKU 키를 길이의 역순으로 정렬
            sorted_sku_keys = sorted(sku_map.keys(), key=len, reverse=True)
            
            formatted = []
            for row in results:
                send_at_dt = datetime.fromtimestamp(row.send_at) if isinstance(row.send_at, int) else row.send_at
                received_at_dt = datetime.fromtimestamp(row.received_at) if isinstance(row.received_at, int) else row.received_at
                
                mail_tag_original = row.mail_tag
                if mail_tag_original == 'subscribe_reward':
                    mail_tag_processed = '구독수령: ' + row.items
                else:
                    mail_tag_processed = mail_tag_original
                
                if isinstance(mail_tag_original, str) and mail_tag_original.startswith('inapp package'):
                    match = re.search(r'\((\d+)\)', mail_tag_original)
                    if match:
                        sku_key_from_tag = str(match.group(1)) # 태그에서 숫자(SKU) 추출
                        
                        # startswith 로직으로 상품명 찾기
                        product_name = None
                        for key in sorted_sku_keys:
                            if sku_key_from_tag.startswith(key):
                                product_name = sku_map[key]
                                break
                        
                        if product_name:
                            mail_tag_processed = product_name
                            logging.info(f"Mail tag translation SUCCESS: '{mail_tag_original}' -> '{product_name}'")
                            
                        else:
                            # 매칭 실패 시 원본 태그를 그대로 사용
                            logging.warning(f"Mail tag translation FAILED for SKU key: '{sku_key_from_tag}'. Not found in map.")
                #
                # ##########################################################################
                # ### [수정 끝] 우편함 SKU 'startswith' 매칭 로직 ###
                # ##########################################################################


                formatted.append({
                    "send_at_str": send_at_dt.strftime('%Y-%m-%d %H:%M:%S') if send_at_dt else 'N/A',
                    "send_at_obj": send_at_dt,
                    "received_at": received_at_dt.strftime('%Y-%m-%d %H:%M:%S') if received_at_dt else 'N/A',
                    "receive_status": "완료" if row.is_receive == 1 else "미수령",
                    "is_receive_val": row.is_receive,
                    "delete_status": "Y" if row.is_delete == 1 else "N",
                    "mail_tag": mail_tag_processed, # 처리된 태그 값 사용                    
                    "idx_no":row.idx_no,
                })
            return formatted
    except (exc.SQLAlchemyError, AttributeError, TypeError) as e:
        logging.error(f"DB/Processing ERROR while fetching mailbox: {e}")
        return None

def search_magnite(engine, user_no):
    """
    user_no로 마그나이트 입출조회하는 함수.
    """
    logging.info(f"Searching magnite for user_no '{user_no}'.")
    try:
        with engine.connect() as conn:        
            query = text("SELECT  `mn_idx` as `순번`,  `magnite` as `현재 보유량`,  `order_amount` as `변동량`, `order_desc` as `변동사유`,  `registed_at` as `변동시간` FROM `user_magnite` WHERE user_no = :user_no ORDER BY `mn_idx` DESC LIMIT 1000")
            result = conn.execute(query, {"user_no": user_no}).fetchall()
            if result:
                formatted = {
                    "key": list(result[0]._mapping.keys()),
                    "val": [list(r._mapping.values()) for r in result]
                }                
                return formatted
    except (exc.SQLAlchemyError, AttributeError, TypeError) as e:
        logging.error(f"DB/Processing ERROR while fetching magnite: {e}")
        return None
        
DUMMY_USERS = {"보안": "보안"}
def login_required(f):
    @wraps(f)
    def decorated_function(*args, **kwargs):
        if 'logged_in' not in session:
            return redirect(url_for('login'))
        return f(*args, **kwargs)
    return decorated_function

# [추가] 계정 차단 라우트
@app.route('/block-user/<int:user_no>')
@login_required
def block_user(user_no):
    db_name = request.args.get('db_name')
    if not db_name:
        flash("DB 정보가 없어 유저를 차단할 수 없습니다.", "danger")
        return redirect(url_for('payment_management'))

    game_engine_rw = get_db_engine_rw(db_name)
    account_engine_rw = get_db_engine_rw('mtd-account')

    if not game_engine_rw or not account_engine_rw:
        flash("쓰기 DB 연결 정보가 없어 작업을 수행할 수 없습니다. (DB_HOST_RW 환경변수 확인)", "danger")
        return redirect(url_for('payment_management', **request.args))

    try:
        # Game DB 트랜잭션
        with game_engine_rw.connect() as conn:
            with conn.begin(): # 트랜잭션 시작
                query = text("UPDATE user_base SET state = 8 WHERE user_no = :user_no")
                result = conn.execute(query, {"user_no": user_no})
                if result.rowcount == 0:
                     raise Exception(f"Game DB({db_name})에서 user_no {user_no}를 찾지 못했습니다.")
                logging.info(f"SUCCESS: Blocked user {user_no} in game DB '{db_name}'.")

        # Account DB 트랜잭션
        with account_engine_rw.connect() as conn:
            with conn.begin(): # 트랜잭션 시작
                query = text("UPDATE user_account SET state = 8 WHERE user_no = :user_no")
                result = conn.execute(query, {"user_no": user_no})
                if result.rowcount == 0:
                    # 계정 DB에 없는 유저(게스트 등)일 수 있으므로 경고만 로깅하고 넘어감
                    logging.warning(f"User {user_no} not found in Account DB(mtd-account), but blocking process continues.")
                else:
                    logging.info(f"SUCCESS: Blocked user {user_no} in account DB 'mtd-account'.")

        flash(f"유저({user_no})가 성공적으로 차단되었습니다.", "success")

    except Exception as e:
        logging.error(f"Failed to block user {user_no}. ERROR: {e}")
        flash(f"유저({user_no}) 차단 중 오류가 발생했습니다: {e}", "danger")

    # 이전 검색 결과 페이지로 리다이렉트. 쿼리 파라미터를 그대로 넘겨줌
    return redirect(url_for('payment_management', **request.args))

@app.route('/mailbox/delete', methods=['POST'])
@login_required
def delete_mailbox():
    idx_no = request.form.get('idx_no')
    db_name = request.form.get('db_name')
    search_query = request.form.get('search_query')
    search_type = request.form.get('search_type')
    tab = request.form.get('tab', 'mailbox')

    if not idx_no:
        flash('잘못된 요청입니다.', 'danger')
        return redirect(url_for(
            'payment_management',
            search_query=search_query,
            search_type=search_type,
            db_name=db_name,
            tab=tab
        ))

    engine = get_db_engine_rw(db_name)
    
    try:
        with engine.connect() as conn:
            conn.execute(
                text("""
                    UPDATE mail_box
                    SET is_delete = 1
                    WHERE idx_no = :idx_no
                """),
                {"idx_no": idx_no}
            )
            conn.commit()

        flash('우편이 삭제되었습니다.', 'success')

    except Exception as e:
        logging.error(f"Mailbox delete error: {e}")
        flash('우편 삭제 중 오류가 발생했습니다.', 'danger')

    return redirect(url_for(
        'payment_management',
        search_query=search_query,
        search_type=search_type,
        db_name=db_name,
        tab=tab
    ))
   
    
@app.route('/update-sku')
@login_required
def update_sku_route():
    """SKU 업데이트 버튼 클릭 시 호출되는 라우트"""
    success, message = update_sku_data()
    if success:
        flash(message, 'success')
    else:
        flash(message, 'danger')
    
    return redirect(url_for('payment_management',
        search_query=request.args.get('search_query'),
        search_type=request.args.get('search_type'),
        db_name=request.args.get('db_name'),
        tab=request.args.get('tab', 'payment')
    ))

@app.route('/', methods=['GET', 'POST'])
@login_required
def payment_management():
    search_query = request.form.get('search_query') or request.args.get('search_query')
    search_type = request.form.get('search_type') or request.args.get('search_type', 'nickname')
    db_name = request.form.get('db_name') or request.args.get('db_name', 'mtd-game-2')
    active_tab = request.args.get('tab', 'payment')
    
    payment_results, mailbox_results, magnite_results, error_message, user_info = None, None, None, None, None
    searched_order_id = None # [수정] 하이라이트를 위한 변수 추가
    
    highlight_time_str = request.args.get('highlight_time')
    highlight_time_obj = None
    if highlight_time_str:
        try:
            highlight_time_obj = datetime.fromisoformat(highlight_time_str)
        except (ValueError, TypeError):
            logging.warning(f"Invalid highlight_time format: {highlight_time_str}")

    sku_map = load_sku_map()

    if search_query:
        # [수정] 주문번호 검색 시, 해당 주문번호를 기억
        if search_type == 'order_id':
            searched_order_id = search_query

        engine = get_db_engine(db_name)
        try:
            with engine.connect() as conn:
                user_no_found = None
                # 1. user_no 찾기
                if search_type == 'order_id':
                    order_id_query = text("SELECT user_no FROM in_app_purchase_test WHERE order_id = :order_id LIMIT 1")
                    result = conn.execute(order_id_query, {"order_id": search_query}).fetchone()
                    if result:
                        user_no_found = result[0]
                        logging.info(f"SUCCESS: Found user_no '{user_no_found}' for order_id '{search_query}'.")
                    else:
                        logging.warning(f"INFO: Order ID '{search_query}' not found.")
                else:  # nickname 또는 user_id
                    user_no_found = find_user_no(conn, search_type, search_query)

                # 2. user_no를 찾았다면, 유저 상세 정보(닉네임 포함) 가져오기
                if user_no_found:
                    user_info_query = text("SELECT u.user_no, u.nick, u.state, g.guild_name FROM user_base AS u LEFT JOIN user_guild AS ug ON u.user_no = ug.user_no LEFT JOIN guild AS g ON ug.guild_no = g.guild_no WHERE u.user_no = :user_no;")
                    user_result = conn.execute(user_info_query, {"user_no": user_no_found}).fetchone()
                    if user_result:
                        user_info = {'user_no': user_result[0], 'nick': user_result[1], 'state': user_result[2], 'guild_name': user_result[3]}
                        logging.info(f"SUCCESS: Fetched user details: {user_info}")
                        # ##########################################################################
                        # ### [수정 시작] 계정 타입 조회 기능 추가 ###
                        # ##########################################################################
                        try:
                            # mtd-account DB에 연결하기 위한 새 엔진 생성
                            account_engine = get_db_engine('mtd-account')
                            with account_engine.connect() as account_conn:
                                account_type_query = text("SELECT a_type FROM user_account_sub WHERE user_no = :user_no")
                                account_result = account_conn.execute(account_type_query, {"user_no": user_no_found}).fetchone()

                                if account_result:
                                    a_type = account_result[0]
                                    if a_type == 1:
                                        user_info['account_type'] = 'Google'
                                    elif a_type == 2:
                                        user_info['account_type'] = 'Apple'
                                    elif a_type == 3:
                                        user_info['account_type'] = 'Immutable'
                                    else:
                                        user_info['account_type'] = f'Unknown ({a_type})'
                                else:
                                    user_info['account_type'] = 'Guest' # 조회되지 않으면 게스트
                                logging.info(f"SUCCESS: Found account type '{user_info['account_type']}' for user_no '{user_no_found}'.")

                        except exc.SQLAlchemyError as e:
                            logging.error(f"DB ERROR while checking account type: {e}")
                            user_info['account_type'] = '타입 조회 실패'
                        # ##########################################################################
                        # ### [수정 끝] 계정 타입 조회 기능 추가 ###
                        # ##########################################################################
                        
                        # 3. 결제 및 우편함 데이터 조회
                        payment_results = search_payment_history_by_userno(engine, user_no_found, sku_map)
                        mailbox_results = search_mailbox(engine, user_no_found, sku_map) # [수정] sku_map 전달
                        magnite_results = search_magnite(engine, user_no_found) # [추가]
                        if payment_results is None or mailbox_results is None:
                            error_message = "데이터 조회 중 오류가 발생했습니다. 로그를 확인해주세요."
                    else:
                        logging.warning(f"Could not find user details for user_no {user_no_found}, though it was found.")
                        error_message = f"유저({user_no_found})를 찾았으나, 상세 정보를 가져올 수 없습니다."
            
            if not user_info and not error_message:
                payment_results, mailbox_results, magnite_results = [], [], []

        except exc.SQLAlchemyError as e:
            logging.error(f"DB ERROR: {e}")
            error_message = "데이터베이스 연결 또는 쿼리 중 오류가 발생했습니다."

    return render_template('payment_management.html',
        search_query=search_query, search_type=search_type, db_name=db_name,
        user_info=user_info, 
        payment_results=payment_results,
        mailbox_results=mailbox_results, 
        magnite_results=magnite_results,
        error_message=error_message,
        active_tab=active_tab, 
        highlight_time=highlight_time_obj,
        searched_order_id=searched_order_id # [수정] 템플릿으로 전달
    )
    
@app.route('/metrics', methods=['GET'])
def metrics():
    engine = get_db_engine()
    tab = request.args.get('tab')

    # ================== 공통 날짜 ==================
    start_date = request.args.get("start_date", DEFAULT_START).replace("T", " ")
    end_date = request.args.get("end_date", DEFAULT_END).replace("T", " ")
    
    if not tab:
        return render_template(
            "metrics.html",
            tab=None,
            start_date=start_date,
            end_date=end_date
        )
        
    with engine.connect() as conn:        
        # ================== 매출 지표 ==================
        if tab == 'revenue':
            rows = []
            columns = []
            with engine.connect() as conn:
                result = conn.execute(
                    REVENUE_SQL,
                    {"start_date": start_date, "end_date": end_date}
                ).fetchall()
                if result:
                    columns = result[0]._mapping.keys()
                    rows = [dict(r._mapping) for r in result]

            return render_template(
                "metrics.html",
                tab="revenue",
                start_date=start_date,
                end_date=end_date,
                columns=columns,
                rows=rows
            )

        # ================== 마그나이트 ==================
        elif tab == "magnite":
            magnite_summary = []
            magnite_detail = []
            with engine.connect() as conn:
                summary = conn.execute(
                    MAGNITE_SUMMARY_SQL,
                    {"start_date": start_date, "end_date": end_date}
                ).fetchall()

                detail = conn.execute(
                    MAGNITE_DETAIL_SQL,
                    {"start_date": start_date, "end_date": end_date}
                ).fetchall()

                magnite_summary = [dict(r._mapping) for r in summary]
                magnite_detail = [dict(r._mapping) for r in detail]

            return render_template(
                "metrics.html",
                tab="magnite",
                start_date=start_date,
                end_date=end_date,
                magnite_summary=magnite_summary,
                magnite_detail=magnite_detail
            )
        # ================== 유저별 ==================
        elif tab == "users":
            users_revenue = []
            with engine.connect() as conn:
                res = conn.execute(
                    USERS_REVENUE_SQL,
                    {"start_date": start_date, "end_date": end_date}
                ).fetchall()

                users_revenue = [dict(r._mapping) for r in res]

            return render_template(
                "metrics.html",
                tab="users",
                start_date=start_date,
                end_date=end_date,
                users_revenue=users_revenue
            )    
        else:
            return render_template(
                "metrics.html",
                tab=None,
                start_date=start_date,
                end_date=end_date
            )
            
@app.route("/reservmail")
@login_required
def reservmail():
    # 1. 대상 경로에 파일이 없는 경우 복사 시도
    if not os.path.exists(RESERV_MAIL_CSV_PATH):
        if os.path.exists(RESERV_MAIL_CSV_ORIGIN_PATH): # 원본이 존재하는지 먼저 확인
            try:
                shutil.copy(RESERV_MAIL_CSV_ORIGIN_PATH, RESERV_MAIL_CSV_PATH)
                logging.info(f"Copied CSV from {RESERV_MAIL_CSV_ORIGIN_PATH}")
            except Exception as e:
                logging.error(f"Copy failed: {e}")
                flash("원본 CSV 파일을 복사하는 데 실패했습니다.", "danger")
        else:
            # 원본도 없고 대상도 없는 경우
            logging.error(f"Origin file not found at {RESERV_MAIL_CSV_ORIGIN_PATH}")
            flash(f"원본 파일을 찾을 수 없습니다: {RESERV_MAIL_CSV_ORIGIN_PATH}", "danger")
            # 빈 파일이라도 생성하거나 에러 페이지로 유도
            return render_template("reservmail.html", rows=[], headers=[], next_key=1)

    # 2. 파일 읽기 시도
    try:
        type_row, headers, rows = read_reserv_mail_csv()
    except Exception as e:
        flash("CSV 파일을 읽는 중 오류가 발생했습니다.", "danger")
        return redirect(url_for('payment_management'))

    next_key = get_next_key(rows)
    mail_titles = read_mail_kor_csv()
    rewards = read_post_rewards_csv()
    item_names = read_item_name_csv()
    
    return render_template(
        "reservmail.html",
        headers=headers,
        rows=rows,
        mail_titles=mail_titles,
        rewards=rewards,
        item_names=item_names,
        next_key=next_key
    )
    
def normalize_datetime(val):
    return val.replace("T", " ") + ":00"
    
def read_mail_kor_csv():
    data = {}

    try:
        with open(RESERV_MAIL_TITLE_CSV_PATH, newline="", encoding="utf-8-sig") as f:
            reader = csv.reader(f)

            next(reader)            # 1행: 타입 (버림)
            headers = next(reader)  # 2행: 실제 헤더

            for line in reader:
                if not any(line):
                    continue

                row = dict(zip(headers, line))

                key = row.get('KEY')
                text = row.get('TEXT')

                if key and text:
                    data[str(key)] = text

    except Exception as e:
        logging.error(f"Failed to read mail_kor.csv: {e}")

    return data

def read_post_rewards_csv():
    data = {}

    try:
        with open(RESERV_MAIL_REWARD_CSV_PATH, newline="", encoding="utf-8-sig") as f:
            reader = csv.reader(f)

            next(reader)            # 1행: 타입 (버림)
            headers = next(reader)  # 2행: 실제 헤더

            for line in reader:                
                if not any(line):
                    continue

                row = dict(zip(headers, line))
                key = row.get('GROUP_ID')

                if not key:
                    continue

                # 🔥 key가 없으면 리스트 생성
                if key not in data:
                    data[key] = []

                # 🔥 해당 key의 리스트에 row 추가
                data[key].append(row)

    except Exception as e:
        logging.error(f"Failed to read post_rewards.csv: {e}")

    return data

    
def read_reserv_mail_csv():
    type_row = []
    headers = []
    rows = []

    try:
        with open(RESERV_MAIL_CSV_PATH, newline="", encoding="utf-8-sig") as f:
            reader = csv.reader(f)

            type_row = next(reader)    # 1행: 타입
            headers = next(reader)     # 2행: 헤더

            for line in reader:
                if not any(line):
                    continue
                rows.append(dict(zip(headers, line[:len(headers)])))

    except Exception as e:
        logging.error(f"Failed to read reserv_mail.csv: {e}")

    return type_row, headers, rows


def read_item_name_csv():
    data = {}

    try:
        with open(ITEM_BASE_CSV_PATH, newline="", encoding="utf-8-sig") as f:
            reader = csv.reader(f)

            next(reader)            # 1행: 타입 (버림)
            next(reader)  # 2행: 실제 헤더

            for line in reader:
                if not any(line):
                    continue

                key = line[5];
                text = line[2];

                if key and text:
                    data[str(key)] = text

    except Exception as e:
        logging.error(f"Failed to read mail_kor.csv: {e}")

    return data

    
def get_next_key(rows):
    keys = [int(r['KEY']) for r in rows if r.get('KEY', '').isdigit()]
    return max(keys) + 1 if keys else 1

def update_google_sheet_all(data, sheet_range='reserv_mail!A1'):
    """
    구글 시트에 데이터를 통째로 업데이트합니다.
    data: [ [row1_col1, row1_col2], [row2_col1, row2_col2], ... ] 형태의 리스트
    """
    try:
        creds_path = os.environ.get('GOOGLE_APPLICATION_CREDENTIALS')
        creds = service_account.Credentials.from_service_account_file(
            creds_path, 
            scopes=['https://www.googleapis.com/auth/spreadsheets']
        )
        service = build('sheets', 'v4', credentials=creds)
        
        body = {
            'values': data
        }
        
        # 시트 내용을 밀어넣기 (기존 내용 덮어쓰기)
        service.spreadsheets().values().update(
            spreadsheetId=SHEET_ID,
            range=sheet_range,
            valueInputOption='RAW',
            body=body
        ).execute()
        
        logging.info(f"SUCCESS: Google Sheet updated (Range: {sheet_range})")
        return True
    except Exception as e:
        logging.error(f"Failed to update Google Sheet: {e}")
        return False
        
@app.route("/reservmail/delete", methods=["POST"])
@login_required
def reservmail_delete():
    key = request.form.get("KEY")
    
    # 1. 현재 시트의 전체 데이터 가져오기 (read_reserv_mail_csv를 활용하거나 새로 조회)
    type_row, headers, rows = read_reserv_mail_csv() 
    
    # 2. 메모리 상에서 데이터 수정 (삭제 플래그 처리)
    updated_rows = []
    for row in rows:
        if row.get("KEY") == key:
            row["IS_DELETED"] = "1"
        updated_rows.append(list(row.values())) # 리스트 형태로 변환

    # 3. 구글 시트로 보낼 전체 데이터 구성 (타입행 + 헤더행 + 데이터행)
    final_data = [type_row, headers] + updated_rows

    # 4. 구글 시트 업데이트
    if update_google_sheet_all(final_data, 'reserv_mail!A1'):
        flash("구글 시트에 삭제 예약이 반영되었습니다.", "success")
    else:
        flash("구글 시트 업데이트에 실패했습니다.", "danger")

    return redirect(url_for("reservmail"))

@app.route("/reservmail/add", methods=["POST"])
@login_required
def reservmail_add():
    try:
        type_row, headers, rows = read_reserv_mail_csv()
        
        # 신규 row 데이터 생성
        new_row_dict = {}
        for h in headers:
            val = request.form.get(h, "")
            if h in ['SEND_START', 'SEND_END', 'EXPIRE_AT'] and val:
                val = normalize_datetime(val)
            new_row_dict[h] = val
        
        # 리스트 형태로 변환하여 추가
        data_to_send = [type_row, headers]
        for r in rows:
            data_to_send.append(list(r.values()))
        data_to_send.append(list(new_row_dict.values()))

        # 구글 시트 업데이트
        if update_google_sheet_all(data_to_send, 'reserv_mail!A1'):
            flash("구글 시트에 신규 보상이 추가되었습니다.", "success")
        else:
            flash("구글 시트 업데이트 실패", "danger")

    except Exception as e:
        logging.error(f"Add failed: {e}")
        flash("추가 중 오류 발생", "danger")

    return redirect(url_for("reservmail"))

    
def write_reserv_mail_csv(type_row, headers, rows):
    with open(RESERV_MAIL_CSV_PATH, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)

        writer.writerow(type_row)   # 1행 유지
        writer.writerow(headers)    # 2행 유지

        for row in rows:
            writer.writerow([row.get(h, "") for h in headers])



@app.route('/login', methods=['GET', 'POST'])
def login():
    error = None
    if request.method == 'POST':
        username, password = request.form['username'], request.form['password']
        if DUMMY_USERS.get(username) == password:
            session['logged_in'] = True
            session['username'] = username
            return redirect(url_for('payment_management'))
        else:
            error = '아이디 또는 비밀번호가 올바르지 않습니다.'
    return render_template('login.html', error=error)

@app.route('/logout')
def logout():
    session.pop('logged_in', None)
    session.pop('username', None)
    return redirect(url_for('login'))


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=55000, debug=True)

