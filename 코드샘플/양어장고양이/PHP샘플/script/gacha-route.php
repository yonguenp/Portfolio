<?php

req_once(SRC_PATH . '/gacha.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpGacha::REQ_GACHA: // op: 1
{
    $gtype = (int)get_numeric_param('gtype');
    $ad = boolval(get_numeric_param('ad'));
    // tutorial
    // if (GachaType::BASIC_1 == $gtype) {
    //     check_gacha_tutorial();
    // }
    
    // cost
    if (!$gtype) {
        Response::error(ResCode::PARAM_ERR, 'gacha type not specified');
    }

    $info = Data::get_gacha_info($gtype);
    if (empty($info)) {
        Response::error(ResCode::PARAM_ERR, 'no gacha info for ' . $gtype);
    }

    $ctype = $info['cost_type'];
    $cost = $info['cost'];
    $ticket = false;
    if ($ad) {
        if (1 > AdLimiter::get()->get_count(AdLimiter::TYPE_GACHA)) {
            res_gacha_error(ResGacha::ADS_EXHAUSTED);
        }
        $gtype = GachaType::BASIC_1;
    } else {
        $inven = User::GetInventory();
        if ($info['count'] <= $inven->get_item_amount(ITEM_PHOTO_TICKET)) {
            $ticket = true;
        } else if ($cost > User::get_currency($ctype)) {
            res_gacha_error(ResGacha::NOT_ENOUGH_GOLD);
        }
    }

    $deck = User::GetCardDeck();
    if (UserCard::get_max_capacity() < $deck->get_num_cards() + $info['count']) {
        res_gacha_error(ResGacha::TOO_MANY_CARDS);
    }
        
    $res = ['rs' => ResGacha::OK, 'gtype' => $gtype];

    GameDb::startTransaction();

    // dice
    $picks = pick_gacha($gtype);
    $res['res'] = $picks;

    // costs
    if ($ad) {
        AdLimiter::get()->use_count(AdLimiter::TYPE_GACHA);
        $res['ad'] = AdLimiter::get()->get_count(AdLimiter::TYPE_GACHA);
    } else {
        if ($ticket) {
            $inven->update_item_amount(ITEM_PHOTO_TICKET, -1 * $info['count'], Usage::GACHA);
        } else {
            User::update_currency($ctype, -1 * $cost, Usage::GACHA);
        }
    }

    // duplication
    $dupes = take_gacha_with_subid($picks);
    $pts = array_sum($dupes);
    if ($pts) {
        $res['dupes'] = $dupes;
        $rew = ['point' => $pts];
        $res['rew'] = User::receive_rewards($rew, Usage::GACHA);
    }

    H3Logger::log_gacha($gtype, $picks);

    GameDb::commit();

    // result
    Response::mainApiResponse($res);
    Response::ok();
} break; //case OpGacha::REQ_GACHA: // op: 1

case OpGacha::QUERY_AD_COUNT:
{
    $ads = AdLimiter::get();
    $cnt = $ads->get_count(AdLimiter::TYPE_GACHA);
    
    Response::mainApiResponse(['rs' => ResGacha::OK, 'ad' => $cnt]);
    Response::ok();

} break; // case OpGacha::QUERY_AD_COUNT:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'gacha opcode not switched');

// EOF
