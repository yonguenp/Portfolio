<?php

req_once(SRC_PATH . 'plant.php');
validate_session_token();

switch (Response::getOpCode()) {

case OpPlant::HARVEST:
{
    $id = get_numeric_param('id');

    $trs = GameDb::getScopedTransaction();
    
    $res = try_harvest($id);
    if (eResPlant::OK === $res['rs']) {
        $trs->commit();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPlant::HARVEST:

case OpPlant::STATE:
{
    $id = get_numeric_param('id');
    $p = Plant::get($id);
    if (!$p) {
        res_plant_error(eResPlant::INVALID_ID);
    }

    $p->advance(Response::getTs());
    if (ePlantType::FISH_TRAP) {
        $p->save_sql();
    }
    
    $res = $p->get_packet();
    $res['rs'] = eResPlant::OK;

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPlant::STATE:

case OpPlant::REWARD_INFO:
{
    $id = get_numeric_param('id');
    $p = Plant::get($id);
    if (!$p) {
        res_plant_error(eResPlant::INVALID_ID);
    }

    $p->advance(Response::getTs());
    if (ePlantType::FISH_TRAP) {
        $p->save_sql();
    }
    
    $res = ['rs' => eResPlant::OK, 'id' => $id];
    $res['state'] = $p->get_reward_state();
    $res['rew'] = $p->get_reward_array();

    $p->write_state_packet();

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpPlant::REWARD_INFO:

case OpPlant::BOOSTER:
{
    $id = get_numeric_param('id');
    $ad = boolval(get_numeric_param('ad'));

    if (ePlantType::FISH_FARM === $id || ePlantType::FISH_TRAP === $id || ePlantType::GIFT_BASKET === $id) {
        $res = apply_plant_boost($id, $ad);
        Response::mainApiResponse($res);
        Response::ok();
    } else {
        error_log(__FUNCTION__ . ' #1');
        Response::api_error(eResPlant::INVALID_ID);
    }

} break; // case OpPlant::BOOSTER:

default:
    break;

} // switch (Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'plant-route could not switch ' . Response::getOpCode());

// EOF
