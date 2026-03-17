<?php

define("FRIEND_MAX_NUM", 40);
define("FRIEND_GIFT_CURRENCY", eCurrency::POINT);
define("FRIEND_GIFT_AMOUNT", 1);
define("FRIEND_GIFT_GOLD", 7);
define("FRIEND_RECOMMENDED_LIST_NUM", 10);

class opFriend
{
    const FRIEND_LIST = 1;      // 맞팔 상태 유저 목록
    const SENT_LIST = 2;        // 내가 친구 신청한 상대 목록
    const RECEIVED_LIST = 3;    // 나에게 친구 신청한 상대 목로
    const SEARCH_USER = 4;      // 닉네임으로 사람 찾기
    const SEND_REQUEST = 5;     // 친구 요청
    const CANCEL_REQUEST = 6;   // 보낸 친구 요청 취소
    const ACCEPT_REQUEST = 7;   // 친구 요청 수락
    const DECLINE_REQUEST = 8;  // 친구 요청 거절
    const UNFOLLOW = 9;         // 친구 상태 해제
    const SEND_GIFT = 10;       // 선물 보내기
    const RECEIVE_GIFT = 11;    // 선물 받기
    const FRIEND_LIST_UPDATE = 12; // 친구, 받은 친구요청 목록의 갱신 상태 조회
    const RECOMMENDED_LIST = 13; // 추천 친구 목록
    const BLOCKED_LIST = 14;    // 차단 목록 조회
    const BLOCK_USER = 15;      // 차단
    const UNBLOCK_USER = 16;    // 차단 해제
    const ACCUSE_USER = 17;     // 불량 채팅 신고
}

class ResFriend
{
    const OK = 0;
    const NO_SUCH_USER = 1;       // 해당 유저는 없습니다
    const ALREADY_FRIEND = 2;     // 이미 친구
    const ALREADY_SENT = 3;       // 이미 요청함
    const NO_REQUEST_SENT = 4;
    const NO_REQUEST_TAKEN = 5;   // 해당 유저로부터의 친구 요청 없음
    const FRIEND_LIST_FULL = 6;   // 친구 목록 가득참
    const NOT_A_FRIEND = 7;       // 친구가 아님
    const ALREADY_RECEIVED = 8;      // 상대로부터 친구 요청 받음
    const CANNOT_SEND_YET = 9;      // 아직 친구에게 선물을 다시 보낼 수 없음
    const NOTHING_TO_RECEIVE = 10;  // 받을 선물이 없음
    const GIFT_DAILY_LIMITED = 11;  // 선물 받기 일일 한도 초과
    const NOT_BLOCKED = 12;       // 차단하지 않은 유저의 차단 해제를 요청
}

class eFriendState
{
    const NONE = 0;
    const FRIEND = 1;             // 친구 상태
    const REQUEST_SENT = 2;       // 내가 상대에게 요청을 보냄
    const REQUEST_DECLINED = 3;   // 내가 보낸 요청을 상대가 거절함
    const REQUEST_TAKEN = 4;      // 상대가 나에게 친구 요청을 보냄
    const BLOCKED = 5;            // 내가 상대를 차단한 상태
}

class eFriendStateSql
{
    const NONE = 0;
    const ACCEPTED = 1;
    const PENDING = 2;
    const DECLINED = 3;
    const BLOCKED = 5;
}

class eFriendGiftState
{
    const NONE = 0;

    const CAN_SEND = 1 << 0;
    const CAN_RECEIVE = 1 << 1;
}

class eFriendGiftStateSql
{
    const NONE = 0;
    const SENT = 1;
    const RECEIVED = 2;
}

function test_get_user_profiles(int $n) {
    return [];
}

function find_user_by_nickname(string $nick) {
    if (!$nick) {
        return false;
    }

    try {
        $res = GameDbRo::queryFirstRow(
            'SELECT `user_id`, `user_nick` FROM `users` WHERE `user_nick`=%s',
            $nick
        );
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }

    if ($res && $nick === $res['user_nick']) {
        return [
            'uno' => intval($res['user_id']),
            'nick' => $nick,
            'lvl' => 1
        ];
    } else {
        return false;
    }
}

function can_send_friend_request(int $status) {
    switch($status) {
        case eFriendState::FRIEND:
            return ResFriend::ALREADY_FRIEND;

        case eFriendState::REQUEST_SENT:
            return ResFriend::ALREADY_SENT;

        case eFriendState::REQUEST_TAKEN:
            return ResFriend::ALREADY_RECEIVED;
            
        default:
            break;
    }

    return ResFriend::OK;
}

function response_friend_error(int $err, array $param = []) {
    $param['rs'] = $err;
    Response::addApiResponse('', 0, $param);
    Response::ok();
}

function build_gift_flag(int $send_state, int $send_exp, int $rcv_state) {    
    $ret = 0;
    if (eFriendGiftStateSql::SENT > $send_state ||
        (eFriendGiftStateSql::RECEIVED === $send_state && Response::getTs() > $send_exp)) {
        // 보내기 전 상태 || 상대가 받았는데 어제 이전일 때 => 보내기 가능
        $ret |= eFriendGiftState::CAN_SEND;
    }
    if (eFriendGiftStateSql::SENT === $rcv_state) {
        // 상대가 보낸 상태 => 받기 가능
        $ret |= eFriendGiftState::CAN_RECEIVE;
    }
    return $ret;
}

// EOF
