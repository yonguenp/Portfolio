<?php

validate_session_token();
req_once(SRC_PATH . 'post.php');

switch (Response::getOpcode())
{

case OpPost::LIST:
{
    $res = [
        'rs' => eResPost::OK,
        'list' => PostItem::get_list()
    ];
    
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPost::LIST:

case OpPost::GET_ONE:
{
    $uid = get_numeric_param('uid');
    $res = ['uid' => $uid];

    if (!$uid || null === ($item = PostItem::get_item($uid))) {
        Response::api_error(eResPost::NO_SUCH_POST, $res);
    } else if ($item->has_expired()) {
        Response::api_error(eResPost::EXPIRED, $res);
    } else if (ePostState::REWARDED <= $item->get_state()) {
        Response::api_error(eResPost::REWARDED, $res);
    } else if (ePostAttachType::NONE === $item->attach_type) {
        Response::api_error(eResPost::NO_ATTACHMENT, $res);
    }

    $good = $item->attachment_as_good();
    if (!$good) {
        error_log('POST_ROUTE_OP1 #1');
        GameDb::rollback();
        Response::api_error(eResPost::NO_ATTACHMENT, $res);
    }

    $rew = [];
    $good->append_reward_array($rew);
    if (empty($rew)) {
        error_log('POST_ROUTE_OP1 #2');
        GameDb::rollback();
        Response::api_error(eResPost::NO_ATTACHMENT, $res);
    }

    GameDb::startTransaction();
    
    $res['rew'] = User::receive_rewards($rew, Usage::POST);
    
    $item->set_state(ePostState::REWARDED);
    $item->save_state_sql();

    GameDb::commit();

    $res['rs'] = eResPost::OK;

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPost::GET_ONE:

case OpPost::GET_ALL:
{
    $all = PostItem::load_all(USER_POST_BOX_LIMIT);
    $list = [];
    $goods = [];
    foreach ($all as $item) {
        if ($item->has_expired() || ePostState::REWARDED <= $item->get_state()) {
            continue;
        }

        $good = $item->attachment_as_good();
        if (!$good || $good->is_box()) {
            continue;
        }

        $list[] = $item->get_id();
        $goods[] = $good;
    }

    if (empty($goods)) {
        Response::api_error(eResPost::NO_ATTACHMENT);
    }

    $rew = [];
    foreach ($goods as $good) {
        if (!$good->append_reward_array($rew)) {
            error_log('failed to attach ' . $good->debug_string());
        }
    }

    $res = [];
    $res['list'] = $list;

    GameDb::startTransaction();

    if (!PostItem::set_multiple_state($list, ePostState::REWARDED)) {
        GameDb::rollback();
        Response::api_error(eResPost::SERVER_ERROR);
    }

    $res['rew'] = User::receive_rewards($rew, Usage::POST);
    User::SaveBaseInfo();

    GameDb::commit();

    $res['rs'] = eResPost::OK;
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPost::GET_ALL:

} // switch (Response::getOpcode())

Response::error(ResCode::PARAM_ERR, 'post-route not implemented for ' . Response::getOpcode());

// EOF
