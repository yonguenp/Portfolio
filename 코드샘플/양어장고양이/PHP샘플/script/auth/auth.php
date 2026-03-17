<?php

// 'auth' api에서 사용될 기능

class OpAuth
{
    const GET_INFO = 1;
    const SIGNUP = 2;
    const SIGNIN_WITH_JWT = 3;
    const SIGNIN_WITH_TOK = 4;
    const SIGNOUT = 5;
    
    // response only
    const SESSION_TOK_UPDATE = 101;
    const USER_INFO = 102;
}

class ResAuth
{
    const OK = 0;
    const AUTH_FAILED = 1;
    const TOKEN_EXPIRE = 2;
    const ACCOUNT_ALREADY_EXISTS = 3;
    const ACCOUNT_NOT_EXISTS = 4;
}

function auth_samanda_token(string $tok) {
    $uri = Cfg::getValue('samandaauth');
    $res = send_curl_request($uri, ['pid' => H3_PID, 'jwt' => $tok]);
    
    return $res;
}

function send_curl_request($uri, $data) {
    $ch = curl_init($uri);
    curl_setopt($ch, CURLOPT_URL, $uri);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER,true);
    curl_setopt($ch, CURLOPT_HEADER, false); 
    curl_setopt($ch, CURLOPT_POST, 1);
    curl_setopt($ch, CURLOPT_HTTPHEADER, array(
        'Content-Type: application/x-www-form-urlencoded'));
    curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query($data));  

    $res = curl_exec($ch);    
    curl_close($ch);

    if(!!$res) {
        return json_decode($res, true);
    } else {
        return FALSE;
    }

}

/**
 * SQL account table에서 Samanda 계정 번호와 연동된 게임 유저 번호를 찾음
 *
 * @param integer $ano Samanda account no
 * @return void haha ha User no. 오류시 ResponseError, 미존재시 0
 */
function get_user_no_from_ano(int $ano) : int {
    if (!intval($ano)) {
        Response::error(ResCode::PARAM_ERR, '', new Exception('!intval($ano)'));
    }

    try {
        $ret = AccountDb::queryFirstField(
            'SELECT `user_no` FROM `accounts` WHERE `account_id`=%i',
            $ano
        );
        
        return intval($ret);
    } catch(Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }
}

function parse_jwt_payload(string $jwt) {
    $bpay = explode('.', $jwt)[1];
    if(!$bpay) {
        return null;
    }

    $bpay = str_replace('-', '+', $bpay);
    $bpay = str_replace('_', '/', $bpay);

    return json_decode(base64_decode($bpay), true);
}

function create_user_account(int $ano) {
    AccountDb::insertIgnore(
        'accounts',
        ['account_id' => $ano]
    );
    return AccountDb::insertId();
}

function response_auth_error(int $opcode, int $rs) {
    Response::addApiResponse('auth', $opcode, ['rs' => $rs]);
    Response::ok();
}

function get_jwt_if_valid() {
    $jwt = get_param('jwt');
    
    if ($jwt && auth_samanda_token($jwt)) {
        return $jwt;
    } else {
        response_auth_error(Response::getOpcode(), ResAuth::AUTH_FAILED);
    }
}

function get_jwt_auth_result() {
    $jwt = get_param('jwt', false);
    if (!$jwt || !($ret = auth_samanda_token($jwt))) {
        response_auth_error(Response::getOpcode(), ResAuth::AUTH_FAILED);
    }

    if (!is_array($ret) || !isset($ret['rs']) || 0 != $ret['rs']) {
        response_auth_error(Response::getOpcode(), ResAuth::AUTH_FAILED);
    }

    return $ret;
}

function create_new_user(int $uno, int $ano, string $nick, int $lang) {
    GameDb::startTransaction();

    // basic user row
    GameDb::insertIgnore(
        'users',
        [
            'user_id' => $uno,
            'sb_ano' => $ano,
            'user_nick' => $nick,
            'locale' => $lang ? $lang : LOCALE_CODE_DEFAULT,
            'level' => 0,
            'visit_checked' => Response::getTs()
        ]
    );

    // success
    if (0 == GameDb::affectedRows()) {
        Response::devLog('0 == GameDb::affectedRows()');
        // already exists?
        $db_ano = GameDb::queryFirstField(
            'SELECT `sb_ano` FROM `users` WHERE `user_id`=%i',
            $uno
        );
        Response::devLog('$db_ano = ' . $db_ano);
        if ($db_ano != $ano) {
            Response::error(ResCode::SQL_ERROR);
        }

        return false;
    }

    // food
    //insert_new_user_food($uno);

    // item
    insert_new_user_item($uno);

    // interaction
    //insert_new_user_inter($uno);

    // cat
    // if (($cat = CatData::get_new_cat_for_level(1))) {
    //     $cat->insert_sql($uno);
    // }
    // neco
    $neco = Neco::create(eNecoId::GILMAK, eNecoState::READY);
    $neco->save_sql($uno);

    // obj
    insert_new_user_objects($uno);

    // plant
    initialize_user_plants($uno);
    // req_once(SRC_PATH . 'plant.php');
    // initialize_user_plant($uno);
    
    GameDb::commit();
    return true;
}

function insert_new_user_inter(int $uno) {
    try {
        // touch
        GameDb::insert(
            'user_inter_touch',
            [
                ['user_id' => $uno, 'touch_id' => 1, 'today_run_count' => 0]
            ]
        );

        // play
        GameDb::insert(
            'user_inter_play',
            [
                ['user_id' => $uno, 'play_id' => 1, 'today_run_count' => 0]
            ]
        );

    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, $e->getMessage());
    }
}

function initialize_user_plants(int $uno) {
    $farm = Plant::create(ePlantType::FISH_FARM);
    $farm->save_sql($uno);

    $trap = Plant::create(ePlantType::FISH_TRAP);
    $trap->save_sql($uno);

    $basket = Plant::create(ePlantType::GIFT_BASKET);
    $basket->save_sql($uno);
}

function insert_new_user_objects(int $uno) : void {
    // initial objects
    $objects = [
        eObjectType::FEED_BOWL,
        eObjectType::FEED_BOWL2,
        eObjectType::FEED_BOWL3,
        eObjectType::FEED_BOWL4,
        eObjectType::FEED_BOWL5,
        eObjectType::FEED_BOWL6,
        eObjectType::FEED_BOWL8,
        eObjectType::FEED_BOWL9,
        eObjectType::FEED_BOWL10

    ];
    
    foreach ($objects as $oid) {
        $obj = SpotObject::create($oid);
        if (!$obj) {
            Response::error(ResCode::SERVER_ERROR, 'failed to create object #' . $oid);
        }
        $obj->save_sql($uno);
    }
}

// function insert_new_user_food(int $uno) {
//     try {
//         GameDb::insert(
//             'user_food',
//             [
//                 ['user_id' => $uno, 'food_id' => 1, 'food_amount' => 10],
//                 ['user_id' => $uno, 'food_id' => 2, 'food_amount' => 10],
//                 ['user_id' => $uno, 'food_id' => 3, 'food_amount' => 10]
//             ]
//         );
//     } catch(Exception $e) {
//         Response::error(ResCode::SQL_ERROR, $e->getMessage());
//     }
// }

function insert_new_user_item(int $uno) {
    try {
        $ins = [];
        $gold = 0;
        $catnip = 0;
        $point = 0;
        $data = DataRedis::get_db('character_init_item')->get_all();
        if (empty($data)) {
            return;
        }
        
        foreach ($data as $row) {
            switch ($row['type']) {
                case 'item':
                    $ins[] = [
                        'user_id' => $uno,
                        'item_id' => intval($row['param']),
                        'get_amount' => intval($row['value'])
                    ];
                    break;

                case 'gold':
                    $gold += intval($row['value']);
                    break;

                case 'catnip':
                    $catnip += intval($row['value']);
                    break;

                case 'point':
                    $point += intval($row['value']);
                    break;

                default:
                    break;
            }
        }

        if (!empty($ins)) {
            GameDb::insert(
                'user_items',
                $ins
            );
        }

        if (0 < $gold + $catnip + $point) {
            GameDb::update(
                'users',
                [
                    'gold' => $gold,
                    'catnip' => $catnip,
                    'point' => $point
                ],
                '`user_id`=%i', $uno
            );
        }
    } catch(Exception $e) {
        Response::error(ResCode::SQL_ERROR, $e->getMessage());
    }
}

function insert_new_user_card(int $uno) {
    try {
        GameDb::insert(
            'user_card',
            [
                ['user_id' => $uno, 'card_id' => 1, 'card_lv' => 1, 'card_exp' => 0, 'card_amount' => 1],
                ['user_id' => $uno, 'card_id' => 2, 'card_lv' => 1, 'card_exp' => 0, 'card_amount' => 1]
            ]
        );
    } catch(Exception $e) {
        Response::error(ResCode::SQL_ERROR, $e->getMessage());
    }
}

function create_session_token() {
    return str_replace('+', '-', str_replace('/', '_', base64_encode(random_bytes(15))));
}

function set_session_token($uno, $tok) {
    SessionToken::set($uno, $tok);
    // GameDb::insertUpdate(
    //     'user_session_token',
    //     ['user_id' => $uno, 'user_token' => $tok],
    //     ['user_token' => $tok]
    // );

    Response::addApiResponse('auth',
        OpAuth::SESSION_TOK_UPDATE,
        ['rs' => ResAuth::OK, 'tk' => $tok]);
}

function emergency_account_reset(int $ano) : void {
    AccountDb::query('DELETE FROM `accounts` WHERE `account_id`=%i', $ano);
}

// EOF
