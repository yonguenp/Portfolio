<?php

req_once(SRC_PATH . 'card/card.php');

switch(Response::getOpCode()) {

case OpCard::WRITE_MEMO:
{
    $uid = get_numeric_param('uid');
    $memo = get_param('memo');

    $deck = User::GetCardDeck();
    $card = $deck->get_card($uid);
    $rs = ResCard::OK;
    if (!$card) {
        $rs = ResCard::INVALID_UID;
    } else if (!empty($card->get_memo())) {
        $rs = ResCard::CANNOT_OVERWRITE;
    } else if (!is_card_memo_usable($memo)) {
        $rs = ResCard::INVALID_MEMO;
    } else {
        $card->set_memo($memo);
        $card->save_sql();
    }

    Response::addApiResponse('', 0, [
        'rs' => $rs, 'uid' => $uid, 'memo' => $memo
    ]);
    Response::ok();

} break; // case OpCard::WRITE_MEMO:

case OpCard::ABANDON_CARD:
{
    //$uid = get_numeric_param('uid');
    $uid = explode(',', get_param('uid'));
    $uid = array_unique($uid);
    if (empty($uid)) {
        Response::error(ResCode::PARAM_ERR);
    }

    // points
    $points = 0;

    // 실제 보유 여부 검증
    $deck = User::GetCardDeck();
    for($i = 0; count($uid) > $i; ++$i) {
        $uid[$i] = intval($uid[$i]);
        $card = $deck->get_card($uid[$i]);
        if ($card) {
            if ($card->is_perfect()) {
                $points += CARD_DUPE_POINT_PERFECT;
            }
        } else {
            Response::addApiResponse('', 0, [
                'rs' => ResCard::INVALID_UID, 'uid' => $uid[$i]
            ]);
            Response::ok();
        }
    }

    GameDb::startTransaction();
    if (!$deck->delete_card($uid, Usage::USER_DID)) {
        GameDb::rollback();
        Response::error(ResCode::SERVER_ERROR);        
    }

    $res = ['rs' => ResCode::OK, 'uid' => $uid];

    if (0 < $points) {
        $g = new Good(eGoodsType::CURRENCY, eCurrency::POINT, $points);
        $rew = [];
        $g->append_reward_array($rew);
        $res['rew'] = User::receive_rewards($rew);
    }

    GameDb::commit();
    
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpCard::ABANDON_CARD:

case OpCard::EXPAND:
{
    $cur = User::get_deck_expansion();
    // if (CARDDECK_MAX_EXPANSION <= $cur) {
    //     Response::api_error(ResCard::FULLY_EXPANDED);
    // }

    $ctype = eCurrency::CATNIP;
    $cost = get_deck_expansion_cost($cur + 1);
    if ($cost > User::get_currency($ctype)) {
        Response::api_error(ResCard::NOT_ENOUGH_COST);
    }

    GameDb::startTransaction();

    User::set_deck_expansion($cur + 1);
    User::update_currency($ctype, -1 * $cost, Usage::GACHA);
    User::SaveBaseInfo();

    $res = ['rs' => ResCard::OK, 'max' => UserCard::get_max_capacity()];

    GameDb::commit();
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpCard::EXPAND:

case 99:
{
    if (!DEV) {
        break;
    }

    $cid = get_numeric_param('cid', 4);
    $exp = get_numeric_param('exp', 500);
    $card = User::GetCard($cid);
    if (!$card) {
        Response::error(ResCode::PARAM_ERR, 'no card #' . $cid);
    }
    $ret = $card->update_exp($exp, Usage::GACHA);
    Response::addApiResponse('', 0, ['rs' => ResCode::OK, 'change' => $ret]);

    Response::ok();
} break; // case 99:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for card');

// EOF
