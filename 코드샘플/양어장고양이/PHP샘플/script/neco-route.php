<?php

validate_session_token();

req_once(SRC_PATH . 'neco.php');

switch (Response::getOpcode()) {

case OpNeco::TAME_NECO:
{
    $nid = get_numeric_param('id');
    if (!$nid) {
        Response::error(ResCode::PARAM_ERR);
    }

    GameDb::startTransaction();

    VisitManager::advance();

    try_tame_neco($nid);

    $oid = get_numeric_param('oid');
    if ($oid && ($obj = SpotObject::get($oid))) {
        remove_neco_from_object($nid);

        $slot = get_param('slot');
        $type = get_numeric_param('sudden');

        $obj->try_invite_cat_specified($slot, Response::getTs(), $nid, $type, 0);
    } else {
        SpotObject::find_and_remove_neco(Response::getTs(), $nid);
    }

    VisitManager::write_state_packet();
    VisitManager::save_dirty();
    GameDb::commit();

    Response::mainApiResponse(['rs' => ResNeco::OK, 'id' => $nid]);
    Response::ok();

} break; // case OpNeco::TAME_NECO:

case OpNeco::MOVE_MAP:
{
    $param = get_param('map');
    if (!$param) {
        Response::error(ResCode::PARAM_ERR, '[map] parameter was not given');
    }

    $order = parse_map_param($param);
    if (empty($order)) {
        error_log('empty order');
        res_neco_error(ResNeco::NO_CHANGE);
    }

    $trs = GameDb::getScopedTransaction();
    VisitManager::advance();

    $res = do_move_neco($order);

    if (true || ResNeco::OK === $res['rs']) {
        VisitManager::write_state_packet();
        VisitManager::save_dirty();
        $trs->commit();
    } else {
        error_log('rs not ok');
    }
    
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpNeco::MOVE_MAP:

case OpNeco::GREETING_GIFT:
{
    $id = get_numeric_param('id');

    $trs = GameDb::getScopedTransaction();

    $res = take_greeting_gift($id);

    if (ResNeco::OK === $res['rs']) {
        $trs->commit();
    }

    Response::mainApiResponse($res);
    Response::ok();
    
} break; // case OpNeco::GREETING_GIFT:

case OpNeco::CHURU:
{
    $id = get_numeric_param('id');
    $res = ['id' => $id];

    $neco = Neco::get($id);
    if (!$neco || !$neco->is_active()) {
        Response::api_error(ResNeco::NO_SUCH_NECO, $res);
    }

    $inven = User::GetInventory();
    if (1 > $inven->get_item_amount(ITEM_CHURU)) {
        Response::api_error(ResNeco::NOT_ENOUGH_CHURU, $res);
    }
    
    $target = find_churu_target($id);
    if (empty($target)) {
        Response::api_error(ResNeco::NOWHERE_TO_GO, $res);
    }

    $oid = $target['oid'];
    $slot = $target['slot'];
    $type = $target['type'];
    $clip = $target['clip'];
    if (eSuddenType::PHOTO === $type) {
        $clip = SuddenData::pick_random_photo($clip);
    }

    $obj = SpotObject::get($oid);
    if (!$obj) {
        Response::error(ResCode::SERVER_ERROR, 'SpotObject::get(' . $oid . ') returned null');
    }

    $res['oid'] = $oid;

    GameDb::startTransaction();

    // price
    $inven->update_item_amount(ITEM_CHURU, -1, Usage::CAT_ACTION_COST);

    VisitManager::advance();

    remove_neco_from_object($id);
    $obj->try_invite_cat_specified(strval($slot), Response::getTs(), $id, $type, $clip, true);

    VisitManager::write_state_packet();
    VisitManager::save_dirty();
    GameDb::commit();

    $res['rs'] = ResNeco::OK;
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpNeco::CHURU:

default:
    break;
}

Response::error(ResCode::PARAM_ERR);

// EOF
