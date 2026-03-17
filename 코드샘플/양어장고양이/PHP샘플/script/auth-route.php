<?php

req_once(SRC_PATH . 'auth/auth.php');
define('CLIENT_TUTORIAL_FINISH', 22);

switch(Response::getOpcode()) {
    case 0:
    {
        req_once(SRC_PATH . 'user.php');
        on_offerwall_callback();
        exit;
    } break;
    case OpAuth::GET_INFO:
    {
        $payload = get_jwt_auth_result();
        $ano = $payload['ano'];
        $nick = $payload['nick'];

        $un = get_user_no_from_ano($ano);
        if (0 == $un || !User::InitWithUserId($un)) {
            response_auth_error(OpAuth::GET_INFO, ResAuth::ACCOUNT_NOT_EXISTS);
        }

        // 20210531 : 매우 과감한 도전
        if (CLIENT_TUTORIAL_FINISH > User::get_content_unlock_value()) {
            emergency_account_reset($ano);
            response_auth_error(OpAuth::GET_INFO, ResAuth::ACCOUNT_NOT_EXISTS);
        }

        $resp = [];
        $resp['rs'] = ResAuth::OK;
        $resp['un'] = $un;
        $resp['nick'] = User::GetBaseInfo(UserInfoKey::NICK);
        $resp['lvl'] = 1;
        
        Response::addApiResponse('', 0, $resp);
        Response::ok();
    } break; // case OpAuth::GET_INFO:

    case OpAuth::SIGNUP:
    {
        // auth token
        $payload = get_jwt_auth_result();
        $ano = $payload['ano'];
        $lang = get_numeric_param('lang', LOCALE_CODE_DEFAULT);
        if(!$ano) {
            response_auth_error(OpAuth::SIGNUP, ResAuth::AUTH_FAILED);
        }
                
        $uno = create_user_account($ano);
        if(0 == $uno && !($uno = get_user_no_from_ano($ano))) {
            Response::error(ResCode::SQL_ERROR);
        }

        // check if already exists
        $cnt = GameDb::queryFirstField('SELECT COUNT(*) FROM `users` WHERE `user_id`=%i', $uno);
        if (0 < $cnt) {
            response_auth_error(OpAuth::SIGNUP, ResAuth::ACCOUNT_ALREADY_EXISTS);
        }

        GameDb::startTransaction();

        // create
        if (!create_new_user($uno, $ano, $payload['nick'], $lang)) {
            Response::error(ResCode::SQL_ERROR);
        }

        if (!User::InitWithUserId($uno)) {
            Response::error(ResCode::SQL_ERROR);
        }
        $info = User::GetUserInfoPacket();
        Response::addApiResponse('auth', OpAuth::USER_INFO, [ 'rs' => ResAuth::OK, 'char' => $info ]);

        // sign in reward
        $post_sent = PushReward::check_push_reward();

        // post last ts
        if (!$post_sent) {
            PostItem::write_last_post_packet(false);
        }

        // session token
        $tok = create_session_token();
        set_session_token($uno, $tok);

        GameDb::commit();

        // write information
        H3Logger::log_login($uno, $_SERVER['REMOTE_ADDR'], OpAuth::SIGNUP);

        Response::ok();
    } break; // case OpAuth::SIGNUP:

    case OpAuth::SIGNIN_WITH_JWT:
    {
        // auth token
        $payload = get_jwt_auth_result();
        $ano = $payload['ano'];
        if(!$ano) {
            response_auth_error(OpAuth::SIGNUP, ResAuth::AUTH_FAILED);
        }        

        $uno = get_user_no_from_ano($ano);
        if (!$uno || !User::InitWithUserId($uno)) {            
            response_auth_error(OpAuth::SIGNIN_WITH_JWT, ResAuth::ACCOUNT_NOT_EXISTS);
        }

        $locale = get_numeric_param('lang', LOCALE_CODE_DEFAULT);

        Response::addApiResponse('', 0, [ 'rs' => ResAuth::OK ]);

        // write information
        $info = User::GetUserInfoPacket();
        Response::addApiResponse('auth', OpAuth::USER_INFO, [ 'rs' => ResAuth::OK, 'char' => $info ]);
        
        GameDb::startTransaction();

        // sign in reward
        $post_sent = PushReward::check_push_reward();

        // post last ts
        if (!$post_sent) {
            PostItem::write_last_post_packet(false);
        }

        // session token
        $tok = create_session_token();
        User::set_locale(get_numeric_param('lang', LOCALE_CODE_DEFAULT));
        set_session_token($uno, $tok);
        GameDb::commit();

        H3Logger::log_login($uno, $_SERVER['REMOTE_ADDR'], OpAuth::SIGNIN_WITH_JWT);

        Response::ok();
    } break; // case OpAuth::SIGNIN_WITH_JWT:

    case OpAuth::SIGNIN_WITH_TOK:
    {
        // auth session token
        $uno = get_numeric_param('un');        
        $tk = get_param('tk');
        if (!$uno || empty($tk) || $tk !== SessionToken::get($uno)) {
            Response::addApiResponse('', 0, ['rs' => ResAuth::AUTH_FAILED]);
            Response::ok();
        }

        // success
        Response::addApiResponse('', 0, ['rs' => ResAuth::OK]);

        // new token
        $tk = create_session_token();
        GameDb::startTransaction();   

        User::set_locale(get_numeric_param('lang', LOCALE_CODE_DEFAULT));
        set_session_token($uno, $tk);

        // sign in reward
        $post_sent = PushReward::check_push_reward();

        // post last ts
        if (!$post_sent) {
            PostItem::write_last_post_packet(false);
        }

        GameDb::commit();

        H3Logger::log_login($uno, $_SERVER['REMOTE_ADDR'], OpAuth::SIGNIN_WITH_TOK);

        Response::ok();
    } break; // case OpAuth::SIGNIN_WITH_TOK:

    case OpAuth::SIGNOUT:
    {
        // auth session token
        validate_session_token();

        $uno = User::GetNo();
        
        Response::addApiResponse('', 0, [ 'rs'=>ResAuth::OK ]);

        GameDb::startTransaction();
        set_session_token($uno, '');
        GameDb::commit();

        H3Logger::log_login($uno, $_SERVER['REMOTE_ADDR'], OpAuth::SIGNOUT);

        Response::ok();

    } break; // case OpAuth::SIGNOUT:

    case 99:
    {
        $un = create_user_account(2);
        Response::addApiResponse(
            'auth', 99, ['uno' => $un]
        );
        Response::ok();
    } break;

    default:
        Response::error(ResCode::PARAM_ERR);
    break;
}


// EOF
