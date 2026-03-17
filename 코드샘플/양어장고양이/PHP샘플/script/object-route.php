<?php

req_once(SRC_PATH . '/object.php');

validate_session_token();

switch (Response::getOpcode()) {

case OpObject::ASK_STATE:
{    
    GameDb::startTransaction();

    VisitManager::advance();

    VisitManager::save_dirty();

    VisitManager::write_state_packet();
    // $obj_packet = SpotObject::get_all_packets(true);
    // if (!empty($obj_packet)) {
    //     Response::addApiResponse(
    //         'object',
    //         OpObject::OBJECT_UPDATE,
    //         [
    //             'objects' => $obj_packet
    //         ]
    //     );
    // }    

    GameDb::commit();

    VisitManager::dev_log();

    Response::mainApiResponse(['rs' => ResObject::OK]);
    // if (defined('CHEAT_MOD')) {
    //     error_log('elap: ' . (microtime(true) - $_SERVER['REQUEST_TIME_FLOAT']));
    // }
    Response::ok();

} break; // case OpObject::ASK_STATE:

case OpObject::TAKE_GIFT:
{
    GameDb::startTransaction();

    VisitManager::advance();
    
    $basket = Plant::get(ePlantType::GIFT_BASKET);
    if (!$basket) {
        $basket = Plant::create(ePlantType::GIFT_BASKET);
    }

    req_once(SRC_PATH . '/plant.php');
    $res = try_harvest(ePlantType::GIFT_BASKET);
    if (eResPlant::OK === $res['rs'] && !empty($res['rew'])) {
        $res['rew'] = User::receive_rewards($res['rew']);
    }

    VisitManager::save_dirty();
    GameDb::commit();

    VisitManager::write_state_packet();

    Response::mainApiResponse($res);
    Response::ok();
} break; // case OpObject::TAKE_GIFT:

case OpObject::GIFT_LIST:
{
    GameDb::startTransaction();

    $basket = Plant::get(ePlantType::GIFT_BASKET);
    if (!$basket) {
        $basket = Plant::create(ePlantType::GIFT_BASKET);
    }

    $basket->advance(Response::getTs());
    $gifts = $basket->get_gifts_array();

    $basket->save_sql();
    
    GameDb::commit();

    Response::mainApiResponse(['rs' => ResObject::OK, 'gifts' => $gifts]);
    $basket->write_state_packet();
    Response::ok();

} break; // case OpObject::GIFT_LIST:

case OpObject::SUDDEN:
{
    $oid = get_numeric_param('oid');
    $slot = get_numeric_param('slot');

    GameDb::startTransaction();

    VisitManager::advance();
    $obj = SpotObject::get($oid);
    if (!$obj) {
        res_object_err(ResObject::NO_SUCH_OBJECT);
    }
    $visits = $obj->get_visits();
    if (!isset($visits[$slot]) ||
        eSuddenType::NONE === $visits[$slot]['state']) {
        res_object_err(ResObject::NOT_ON_VISIT);
    }

    $res = handle_sudden($obj, $slot);

    if (ResObject::OK === $res['rs']) {
        VisitManager::save_dirty();
        VisitManager::write_state_packet();

        GameDb::commit();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpObject::SUDDEN:

} // switch (Response::getOpcode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for object');

// EOF
