<?php

validate_session_token();

req_once(SRC_PATH . '/user.php');

switch(Response::getOpCode()) {

case OpUser::UNLOCK_CONTENT:
{
    $val = get_numeric_param('val');
    $res = ['val' => $val];
    
    $before = User::get_content_unlock_value();

    if ($before >= $val) {
        Response::mainApiResponse($res);
        Response::ok();
    }
    
    GameDb::startTransaction();

    // tutorial rewards
    $rew = build_tutorial_reward_array($before, $val);
    if ($rew && !empty($rew)) {
        req_once(SRC_PATH . '/chore/chore.php');
        if (!is_chore_moderate($rew)) {
            $res['rew'] = User::receive_rewards($rew, Usage::TUTORIAL);
        }
    }

    // tutorial finish check
    check_tutorial_finish($before, $val);
    
    User::set_content_unlock_value($val);
    Neco::on_tutorial_state_update($before, $val);
    
    GameDb::commit();
    Response::mainApiResponse($res);
    Response::ok();
} break;

case OpUser::READY_CAT:
{
    $val = get_numeric_param('val');
    User::set_ready_cat_value($val);
    User::SaveBaseInfoRedis();

    Response::addApiResponse('', 0, ['val' => $val]);
    Response::ok();
} break;

case OpUser::SET_LOCALE:
{
    if (!($locale = get_numeric_param('lang', false))) {
        Response::error(ResCode::PARAM_ERR);
    }

    GameDb::startTransaction();
    User::set_locale($locale);
    GameDb::commit();

    Response::mainApiResponse(['rs' => ResCode::OK]);
    Response::ok();
    
} break; // case OpUser::SET_LOCALE:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for api auth. #' . Response::getOpCode());

// EOF
