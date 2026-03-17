<?php

define('GACHA_TUTORIAL_FAIL', 42);
define('GACHA_TURORIAL_SUCCESS', 44);

class OpGacha
{
    const REQ_GACHA = 1;
    const QUERY_AD_COUNT = 2;
}

class ResGacha
{
    const OK = 0;
    const NOT_ENOUGH_GOLD = 1;
    const TOO_MANY_CARDS = 2;
    const ADS_EXHAUSTED = 3;
}

class GachaType
{
    const BASIC_1 = 1;
    const BASIC_11 = 2;
}

function check_gacha_tutorial() {
    $ctx = User::GetBaseInfo(UserInfoKey::CONTENTS_UNLOCK);
    if (GACHA_TURORIAL_SUCCESS < $ctx) {
        return;
    }
    
    if (GACHA_TUTORIAL_FAIL < $ctx) {
        define('GACHA_TUTORIAL_OVERRIDE', true);
    } else {
        define('GACHA_TUTORIAL_OVERRIDE', false);
    }
}

function res_gacha_error(int $err) {
    Response::addApiResponse('', 0, ['rs' => $err]);
    Response::ok();
}

function pick_gacha(int $gtype) {
    req_once(LIB_PATH . 'algorithm.php');

    $info = Data::get_gacha_info($gtype);
    $repeat = $info['count'];

    $ret = [];
    for($i = 0; $repeat > $i; ++$i) {
        $card = pick_gacha_from_table($info['proc']);
        $ret[] = pick_subid_for_card($card['id']);
    }

    return $ret;
}

function pick_gacha_from_table($table) {
    $i = lower_bound($table['weight'], mt_rand(1, $table['sum']));
    return $table['cards'][$i];
}

function pick_subid_for_card(int $cid) : int {
    $tb = Data::get_gacha_sub_table($cid);
    if (!$tb) {
        Response::error(ResCode::SERVER_ERROR, 'Card Rect table not exists for card #' . $cid);
    }

    $idx = pick_discrete_distribution($tb['proc']);
    if (isset($tb['uid'][$idx])) {
        return $tb['uid'][$idx];
    } else {
        Response::error(ResCode::SERVER_ERROR, 'discrete distribution fail. seed ' . json_encode($tb['proc']));
    }
}

function take_gacha_with_subid(array $picks) : array {
    $deck = User::GetCardDeck();
    $dupes = [];    

    foreach ($picks as $subid) {
        $info = DataRedis::get_row('card_define_sub', $subid);
        if (!$info) {
            Response::error(ResCode::REDIS_ERROR, 'INVALID CARD UID ' . $subid);
        }
        $pid = intval($info['parent_id']);
        $perfect = intval($info['is_perfect']);
        $rct = ($perfect) ? Card::STR_PERFECT : $info['rect_value'];

        if (null === $deck->get_card($subid)) {
            $dupes[] = 0;
            $deck->obtain_card($subid, $pid, $rct, Usage::GACHA);
        } else {
            $dupes[] = $perfect ? CARD_DUPE_POINT_PERFECT : CARD_DUPE_POINT_PARTIAL;
        }
    }

    return $dupes;
}



// EOF
