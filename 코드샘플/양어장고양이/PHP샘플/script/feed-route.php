<?php

req_once(SRC_PATH . 'feed.php');
validate_session_token();

switch (Response::getOpCode()) {

case OpFeed::FEED:
{
    req_once(SRC_PATH . 'object.php');

    $item_id = get_numeric_param('item');
    //$oid = get_numeric_param('oid', eObjectType::FEED_BOWL);

    $param_oid = get_param('oid', strval(eObjectType::FEED_BOWL));
    $arr_oid = explode(',', $param_oid);

    $trs = GameDb::getScopedTransaction();
    $rs = eResFeed::LACKS_FOOD;
    foreach ($arr_oid as $oid) {
        $oid = intval($oid);
        if (!is_feed_bowl($oid)) {
            continue;
        }

        if (eResFeed::OK === try_feed($item_id)) {
            $rs = eResFeed::OK;
            VisitManager::feed($item_id, $oid);
            Mission::fire(eMissionTriggerType::FEED);
        }
    }

    if (eResFeed::OK === $rs) {
        VisitManager::save_dirty();
        VisitManager::write_state_packet();
        $trs->commit();
    } else {
        GameDb::rollback();
    }

    $res = [];
    $res['item'] = $item_id;
    $res['oid'] = $param_oid;
    $res['rs'] = $rs;

    Response::mainApiResponse($res);
    Response::ok();

} // case OpFeed::FEED:

default:
    break;

} // switch (Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'feed-route could not switch ' . Response::getOpCode());

// EOF
