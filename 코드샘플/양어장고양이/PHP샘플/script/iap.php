<?php

class OpIap
{
    const CHECK = 1;
    const POST_PAYMENT = 2;
    const PURCHASE_COUNT = 3;
    const FULL_INFO = 4;
    const LIMITED_INFO = 5;

    const LIMITED_PRODUCT_OPEN = IAP_OP_LIMITED_OPEN;
}

class eResIap
{
    const OK = 0;
    // 파라미터 관련
    const NO_SUCH_PRODUCT = 1;          // 존재 않는 상품 id를 전달받음
    // 기간 한정
    const CURRENTLY_NOT_AVAILABLE = 2;  // 기간 한정 상품으로 현재 판매 않음
    // 구매 횟수 제한
    const ALREADY_PURCHASED = 3;        // 구매 횟수 제한 상품이며 이미 구매함
    
    // 검증 오류
    const PARAM_ERR = 4;                // 결제 성공 이벤트의 payload를 전달해야 함
    const INVALID_RECEIPT = 5;          // 결제 영수증 검증 실패
    const ALREADY_REWARDED = 6;         // 해당 영수증에 대한 보상이 이미 지급되었음

    // 패스
    const NOT_IN_SEASON = 7;            // 패스 시즌 아님
    const REQUIREMENT_NOT_MET = 8;      // 1단계용 패스 먼저 구매해야 함

    // server err
    const SERVER_ERROR = 9;

    // appending
    const PRODUCT_ID_NOT_MATCH = 10;    // 영수증의 sku와 일치하지 않는 prod_id 호출됨
    const PURCHASE_NOT_MADE = 11;       // 구매 완료 상태가 아닌 영수증
    const VERIFICATION_FAILED = 12;     // 영수증 검증 실패
}

class eMarketPlatform
{
    const GOOGLE_PLAY = 1;
    const APPLE_APPSTORE = 2;
    const FAKE = 9;
}

class eIapState
{
    /**
     * 마켓과 결제가 이루어진 상황
     */
    const PURCHASED = 0;            // 구매 및 보상 지급 성공
    const CANCELED = 1;             // 결제 취소
    const DUPLICATED = 2;           // 결제 성공했으나 기획상 구매 제한 관계로 보상 지급 불가. 환불 대기
    const REWARD_PENDING = 3;       // 기타 이유로 보상 정상 지급 실패

    /**
     * 영수증 검증 실패
     */
    const VERIFY_SERVER_ERROR = 4;  // 
    const NOT_VERIFIED = 5;         // 영수증 구조는 바르나 인증 실패
}

class eIapLimitType
{
    const NONE = 0;
    const FOREVER = 1;
    const DAILY = 2;
    const WEEKLY = 3;
    const SEASONAL = 4;
}

function str_to_iap_limit(string $s) : int {
    switch ($s) {
        case 'account': return eIapLimitType::FOREVER;
        case 'daily':   return eIapLimitType::DAILY;
        case 'weekly':  return eIapLimitType::WEEKLY;
        case 'season':  return eIapLimitType::SEASONAL;
        default: break;
    }

    return eIapLimitType::NONE;
}

function parse_unity_store_string($store) : int {
    switch ($store) {
        case 'fake':            return eMarketPlatform::FAKE;
        case 'GooglePlay':      return eMarketPlatform::GOOGLE_PLAY;
        case 'AppleAppStore':   return eMarketPlatform::APPLE_APPSTORE;
        default:                break;
    }
    return 0;
}

function post_purchase_process(int $prod, array $receipt) : array {
    $ret = ['prod' => $prod];

    $ret['rs'] = eResIap::OK;
    return $ret;
}

function on_purchase_fail(int $code, int $prod, $receipt) : void {

    Response::api_error($code, ['prod' => $prod]);
    exit;
}

function get_iap_period_start(int $ltype) : int {
    switch ($ltype) {
        case eIapLimitType::DAILY:
            return daily_period_start_timestamp();

        case eIapLimitType::WEEKLY:
            return next_datetime_weekly(IAP_WEEKLY_RESET_DAY, IAP_WEEKLY_RESET_HOUR) -
                SEC_DAY * DAY_WEEK;
                
        case eIapLimitType::SEASONAL:
            return PassData::get_season_begin();
            break;
        default:
            break;            
    }
    return 0;
}

function get_iap_period_end(int $ltype) : int {
    switch ($ltype) {
        case eIapLimitType::DAILY:
            return daily_period_start_timestamp() + SEC_DAY;

        case eIapLimitType::WEEKLY:
            return next_datetime_weekly(IAP_WEEKLY_RESET_DAY, IAP_WEEKLY_RESET_HOUR);

        case eIapLimitType::SEASONAL:
            return PassData::get_season_exp();

        case eIapLimitType::FOREVER:
            return 0;

        default:
            break;
    }
    return 0;
}

function handle_iap_reward(int $prod, array &$msg) : int {
    $info = ShopData::get_static_good_info($prod);
    if (!$info) {
        return eResIap::NO_SUCH_PRODUCT;
    }

    switch ($info->type) {
        case eShopGoods::PACKAGE:
            return send_iap_package_mail($info->id);

        case eShopGoods::PASS:
            return apply_iap_pass($info);

        case eShopGoods::GOLD:
        case eShopGoods::CATNIP:
        case eShopGoods::POINT:
        case eShopGoods::ITEM:
        {
            $rew = $info->to_reward_array(1);
            if (!empty($rew)) {
                $msg['rew'] = $rew;
                return eResIap::OK;
            }            
        } break;

        default:
            break;
    }

    return eResIap::NO_SUCH_PRODUCT;
}

function send_iap_package_mail(int $prod) : int {
    req_once(SRC_PATH . 'post.php');
    if (PackageData::send_mail($prod, 0)) {
        return eResIap::OK;
    } else {
        return eResIap::SERVER_ERROR;
    }
}

function apply_iap_pass(ShopItem $info) : int {
    $pass = Pass::get();
    if (!$pass) {
        return eResIap::SERVER_ERROR;
    }

    if ($pass->get_step() > $info->id) {
        return eResIap::ALREADY_PURCHASED;
    } else if ($pass->get_step() < $info->id) {
        return eResIap::REQUIREMENT_NOT_MET;
    }

    $pass->increase_step();

    $package = (2 === $info->id) ?
        IAP_PASS_PACKAGE_2 :
        IAP_PASS_PACKAGE_1;
        
    return send_iap_package_mail($package);
}

function daily_period_start_timestamp(int $ts = 0) : int {
    if (!$ts) {
        $ts = Response::getTs();
    }

    $now = new DateTime();
    $pivot = new DateTime();
    $now->setTimestamp($ts);
    $pivot->setTimestamp($ts);
    $pivot->setTime(IAP_DAILY_RESET_HOUR, 0, 0);

    $diff = 0;
    if ($now < $pivot) {
        $diff = -1 * SEC_DAY;
    }

    return $pivot->getTimestamp() + $diff;
}

// EOF
