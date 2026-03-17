<?php

req_once(SRC_PATH . '/collection/collection.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpCollection::REQ_REWARD:
{
    $coll = User::GetCollection();
    $col_id = get_numeric_param('id');
    
    // check if already done
    if (in_array($col_id, $coll->get_list())) {
        api_collection_error(ResCollection::ALREADY_REWARDED, ['id' => $col_id]);
    }

    // check condition
    if (false == $coll->check_for_condition($col_id)) {
        api_collection_error(ResCollection::REQUIREMENT_NOT_MET, ['id' => $col_id]);
    }

    // success
    GameDb::startTransaction();

    // check data
    $coll->set_rewarded($col_id);

    $rew = $coll->get_reward($col_id);
    
    // todo:: and rewards
    $rew = User::receive_rewards($rew, Usage::COLLECTION_REWARD);
    
    // todo:: first api response
    Response::addApiResponse(
        '',
        0,
        ['rs' => ResCollection::OK, 'id' => $col_id, 'rew' => $rew]
    );

    GameDb::commit();
    Response::ok();
} break; // case OpCollection::REQ_REWARD:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'opcode not switched');

// EOF
