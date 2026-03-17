<?php

class eResCat
{
    const OK = 0;

    const INVALID_CATID = 1;
    const INVALID_ACTIONID = 2;
    const ACTION_NOT_AVAILABLE = 3;
    const NOT_ENOUGH_FULLNESS = 4;
    const NOT_ON_ACTION = 5;
    const NO_MORE_ACTION = 6;
    const NOT_ENOUGH_GOLD = 7;
    const NO_MORE_LEVEL = 8;
    const ITEM_NOT_FOUND = 9;
    const ALREADY_FULL = 10;
    const INVALID_FOOD_ID = 11;
    const NOT_ENOUGH_LEVEL = 12;
    const TOO_MANY_CARDS = 13;
}

// soft error
function response_cat_error(int $error) {
    Response::mainApiResponse(['rs' => $error]);
    Response::ok();
}

// start action
function start_cat_action(int $catid, int $actid) {
    $uid = User::GetNo();
    if (!$uid) {
        Response::error(ResCode::SERVER_ERROR, '!$uid');
    }

    try {
        GameDb::insertUpdate(
            'cat_action',
            [
                'user_id' => $uid,
                'cat_id' => $catid,
                'on_running' => 1,
                'action_id' => $actid
            ],
            [                
                'cat_id' => $catid,
                'on_running' => 1,
                'action_id' => $actid
            ]
        );
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }
}

function get_ongoing_action() {
    $uid = User::GetNo();
    if (!$uid) {
        Response::error(ResCode::SERVER_ERROR, '!$uid');
    }

    $q = 'SELECT `action_id` FROM `cat_action` WHERE `user_id`=%i AND `on_running`=1';
    try {
        $res = GameDb::queryFirstField($q, $uid);
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }

    return intval($res);
}

/**
 * 고양이 행동의 완료 결과 처리
 * GameDb의 cat_action 테이블 완료 처리
 * 고양이 포만감 차감, 애정도 지급
 * 유저 보상은 route에서 진행할 것
 *
 * @return array    response array
 */
function finish_cat_action(object $cat, array $data) : array {
    $ret = ['act' => $data['id']];

    try {
        GameDb::update(
            'cat_action',
            ['on_running' => 0],
            '`user_id`=%i',
            User::GetNo()
        );
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }

    $cat->update_affection($data['affection']);
    $cat->update_fullness(-1 * $data['fullness']);
    $cat->update_sql();

    if (isset($data['rew'])) {
        $ret['rew'] = $data['rew'];
    }

    $ret['rs'] = eResCat::OK;

    return $ret;
}

function open_cat_action(object $cat, array $data) : array {
    // check level
    if ($cat->get_action_step() >= $cat->get_level()) {
        return ['rs' => eResCat::NOT_ENOUGH_LEVEL];
    }

    // check cost
    $cost = $data['cost'];
    if (User::GetGold() < $cost) {
        return ['rs' => eResCat::NOT_ENOUGH_GOLD];
    }

    User::UpdateGold(-1 * $cost, Usage::CAT_ACTION_COST);
    $cat->increase_action_step();
    $cat->update_sql();

    return ['rs' => eResCat::OK, 'cat' => $cat->get_id(), 'act' => $cat->get_action_step(), 'actid' => $data['id']];
}
function increase_cat_level(object $cat, array $data) : array {
    // level check
    if ($cat->get_level() >= $data['max_lvl']) {
        return ['rs' => eResCat::NO_MORE_LEVEL];
    }

    // cost check
    $cost = $data['need_gold'][$cat->get_level() - 1];
    if (User::GetGold() < $cost) {
        return ['rs' => eResCat::NOT_ENOUGH_GOLD];
    }

    User::UpdateGold(-1 * $cost, Usage::CAT_LEVEL_COST);
    $cat->increase_level();
    $cat->update_sql();

    $ret = ['rs' => eResCat::OK, 'cat' => $cat->get_id()];
    $rew = CatData::get_cat_level_reward($cat->get_id(), $cat->get_level());
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }

    return $ret;
}

function feed_cat(object $cat, int $itemno, int $cnt) {
    $cur_full = $cat->get_fullness();
    $max_full = CatData::get_max_fullness($cat->get_id(), $cat->get_level());
    if ($cur_full >= $max_full) {
        return ['rs' => eResCat::ALREADY_FULL];
    }

    $per_item = Data::get_item_fullness($itemno);
    if (!$per_item) {
        return ['rs' => eResCat::INVALID_FOOD_ID];
    }

    // actual cosume count
    $cnt = min($cnt, ceil(($max_full - $cur_full) / $per_item));

    // check item
    if ($cnt > User::GetInventory()->get_item_amount($itemno)) {
        return ['rs' => eResCat::ITEM_NOT_FOUND];
    }

    User::GetInventory()->update_item_amount($itemno, -1 * $cnt, Usage::INTER_FEED);
    $cat->update_fullness($cnt * $per_item);
    $cat->update_sql();

    return [
        'rs' => eResCat::OK,
        'cat' => $cat->get_id()
    ];
}

function take_cat_picture(object $cat) {
    if (UserCard::get_max_capacity() <= User::GetCardDeck()->get_num_cards()) {
        return ['rs' => eResCat::TOO_MANY_CARDS];
    }

    $catid = $cat->get_id();

    // TODO:: check affection ...

    $card_id = CatData::pick_card_for_cat($catid);
    if (!$card_id) {
        Response::error(ResCode::SERVER_ERROR, 'no card picked for cat #' . $catid);
    }
    $card_uid = User::GetBaseInfo('card_sequence') + 1;

    $ret = ['rs' => eResCat::OK, 'cat' => $catid, 'res' => [$card_uid]];
    User::receive_rewards(['card' => [['id' => $card_id]]], Usage::CAT_PICTURE);

    return $ret;
}

// EOF
