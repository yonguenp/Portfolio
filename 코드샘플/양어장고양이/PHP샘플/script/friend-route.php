<?php

req_once(SRC_PATH . 'friend.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpFriend::FRIEND_LIST: 
{
    $list = Friendship::get_friend_list();

    Response::addApiResponse('', 0, [
        'rs' => ResFriend::OK,
        'list' => $list
    ]);

    Response::ok();
} // case OpFriend::FRIEND_LIST:

case OpFriend::SENT_LIST:
{
    $list = Friendship::get_sent_list();

    Response::addApiResponse('', 0, [
        'rs' => ResFriend::OK,
        'list' => $list
    ]);
    Response::ok();
} // case OpFriend::SENT_LIST:

case OpFriend::RECEIVED_LIST:
{
    $list = Friendship::get_received_list();

    Response::addApiResponse('', 0, [
        'rs' => ResFriend::OK,
        'list' => $list
    ]);
    Response::ok();
} // case OpFriend::RECEIVED_LIST:

case OpFriend::SEARCH_USER:
{
    $nick = get_param('nick', false);
    if (false === $nick) {
        Response::error(ResCode::PARAM_ERR);
    }

    $profile = find_user_by_nickname($nick);
    if (!$profile) {
        Response::addApiResponse('', 0, [
            'rs' => ResFriend::NO_SUCH_USER
        ]);
        Response::ok();
    }

    if (User::GetNo() === $profile['uno']) {
        response_friend_error(ResFriend::NO_SUCH_USER);
    }

    $status = Friendship::get_relation($profile['uno']);
    $profile['state'] = $status;

    Response::addApiResponse('', 0, [
        'rs' => ResFriend::OK,
        'user' => $profile
    ]);
    Response::ok();
} // case OpFriend::SEARCH_USER:

case OpFriend::SEND_REQUEST:
{
    $un = get_numeric_param('uno');
    if (!$un || $un === User::GetNo()) {
        Response::error(ResCode::PARAM_ERR);
    }

    $rs = Friendship::send_request($un);
    
    Response::addApiResponse('', 0, [
        'rs' => $rs,
        'uno' => $un
    ]);
    Response::ok();
} // case OpFriend::SEND_REQUEST:

case OpFriend::CANCEL_REQUEST:
{
    $un = get_numeric_param('uno');
    if (!$un || $un === User::GetNo()) {
        Response::error(ResCode::PARAM_ERR);
    }

    $rs = Friendship::cancel_request($un);

    Response::addApiResponse('', 0, [
        'rs' => $rs,
        'uno' => $un
    ]);
    Response::ok();
} // case OpFriend::CANCEL_REQUEST:

case OpFriend::ACCEPT_REQUEST:
{
    $un = get_numeric_param('uno');
    if (!$un || $un === User::GetNo()) {
        Response::error(ResCode::PARAM_ERR);
    }

    $rs = Friendship::accept_request($un);

    Response::addApiResponse('', 0, [
        'rs' => $rs,
        'uno' => $un
    ]);
    Response::ok();
} // case OpFriend::ACCEPT_REQUEST:

case OpFriend::DECLINE_REQUEST:
{    
    $un = get_numeric_param('uno');
    if (!$un || $un === User::GetNo()) {
        Response::error(ResCode::PARAM_ERR);
    }

    $rs = Friendship::decline_request($un);

    Response::addApiResponse('', 0, [
        'rs' => $rs,
        'uno' => $un
    ]);
    Response::ok();
} // case OpFriend::DECLINE_REQUEST:

case OpFriend::UNFOLLOW:
{
    $un = get_numeric_param('uno');
    if (!$un || $un === User::GetNo()) {
        Response::error(ResCode::PARAM_ERR);
    }

    $relation = Friendship::get_relation($un);
    if (eFriendState::FRIEND !== $relation) {
        response_friend_error(ResFriend::NOT_A_FRIEND);
    }

    $rs = Friendship::unfollow($un);

    Response::addApiResponse('', 0, [
        'rs' => $rs,
        'uno' => $un
    ]);
    Response::ok();
} // case OpFriend::UNFOLLOW:

case OpFriend::SEND_GIFT:
{    
    $uno = get_numeric_param('uno');

    $trans = GameDb::getScopedTransaction();
    
    $res = Friendship::try_send_gift($uno);
    if ($res && isset($res['rs']) && ResFriend::OK === $res['rs']) {
        $trans->commit();
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();
} // case OpFriend::SEND_GIFT:

case OpFriend::RECEIVE_GIFT:
{
    $uno = get_numeric_param('uno', 0);
    $trans = GameDb::getScopedTransaction();

    $res = Friendship::try_receive_gift($uno);
    if ($res && isset($res['rs']) && ResFriend::OK === $res['rs']) {
        $res['rew'] = User::receive_rewards($res['rew'], Usage::FRIEND_GIFT);
        $trans->commit();
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();
} // case OpFriend::RECEIVE_GIFT:

case OpFriend::FRIEND_LIST_UPDATE:
{
    $cnt = Friendship::get_new_friend_number();
    
    $cnt['rs'] = ResFriend::OK;
    Response::addApiResponse('', 0, $cnt);
    Response::ok();
} // case OpFriend::FRIEND_LIST_UPDATE:

case OpFriend::RECOMMENDED_LIST:
{
    $list = Friendship::get_recommended_list();

    Response::mainApiResponse([
        'rs' => ResFriend::OK,
        'list' => $list
    ]);

    Response::ok();
} break; // case OpFriend::RECOMMENDED_LIST:

case OpFriend::BLOCKED_LIST:
{
    $list = Friendship::get_blocked_list();

    Response::mainApiResponse([
        'rs' => ResFriend::OK,
        'list' => $list
    ]);

    Response::ok();
} break; // case OpFriend::BLOCKED_LIST:

case OpFriend::BLOCK_USER:
{
    $uno = get_numeric_param('uno');
    $res = [];
    $res['uno'] = $uno;

    if (0 == $uno || User::GetNo() == $uno) {
        Response::api_error(ResFriend::NO_SUCH_USER, $res);
    }

    $res['rs'] = Friendship::block_user($uno);

    Response::mainApiResponse($res);
    Response::ok();
    
} break; // case OpFriend::BLOCK_USER:

case OpFriend::UNBLOCK_USER:
{
    $uno = get_numeric_param('uno');
    $res = [];
    $res['uno'] = $uno;

    if (0 == $uno || User::GetNo() == $uno) {
        Response::api_error(ResFriend::NO_SUCH_USER, $res);
    }

    $res['rs'] = Friendship::unblock_user($uno);

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpFriend::UNBLOCK_USER:

case opFriend::ACCUSE_USER:
{
    Response::mainApiResponse(['rs' => ResFriend::OK]);
    Response::ok();
} break; // case OpFriend::ACCUSE_USER:

default:
break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for friend');

// EOF
