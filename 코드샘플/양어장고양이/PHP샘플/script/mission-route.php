<?php

req_once(SRC_PATH . '/mission.php');

validate_session_token();

switch (Response::getOpcode()) {

case OpMission::ASK_PASS_STATE:
{
    GameDb::startTransaction();

    $ret = Pass::get_packet();
    $ret['rs'] = eResMission::OK;

    GameDb::commit();

    Response::mainApiResponse($ret);
    Response::ok();

} break; // case OpMission::ASK_PASS_STATE:

case OpMission::GET_PASS_REWARD:
{
    // optional numeric param 'level' : 특정 레벨 보상 수령. 0이면 수령 가능한 모든 보상
    $level = get_numeric_param('level', 0);
    $step = get_numeric_param('step', 0);

    $trs = GameDb::getScopedTransaction();

    $res = receive_pass_rewards($level, $step);
    
    if (eResMission::OK === $res['rs']) {
        $trs->commit();        
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpMission::GET_PASS_REWARD:

case OpMission::USE_PASS_TICKET:
{
    // No parameter required.
    
    $trs = GameDb::getScopedTransaction();

    $res = use_pass_ticket();
    if (eResMission::OK === $res['rs']) {
        $trs->commit();
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpMission::USE_PASS_TICKET:

case OpMission::ASK_MISSION_STATE:
{
    $type = get_numeric_param('type', 0);

    $missions = Mission::get_state_packet($type);

    Response::mainApiResponse([
        'rs' => eResMission::OK,
        'type' => $type,
        'missions' => $missions
    ]);
    Response::ok();

} break; // case OpMission::ASK_MISSION_STATE:

case OpMission::GET_MISSION_REWARD:
{
    $mid = get_numeric_param('mid', 0);

    $trs = GameDb::getScopedTransaction();

    if ($mid) {
        // $rs = Mission::finish($mid);
        // $res = ['rs' => $rs, 'mid' => $mid];
        $res = Mission::finish($mid);
    } else {
        $type = get_numeric_param('type', 0);
        // $rs = Mission::finish_all($type);
        // $res = ['rs' => $rs, 'type' => $type];
        $res = Mission::finish_all($type);
    }
    
    if (eResMission::OK === $res['rs']) {
        $packet = Mission::get_dirty_packet();
        if (!empty($packet)) {
            Response::addApiResponse(
                'mission',
                OpMission::MISSION_STATE_UPDATE,
                ['missions' => $packet]
            );
        }

        if (isset($res['rew'])) {
            $res['rew'] = User::receive_rewards($res['rew'], Usage::MISSION);
        }

        Mission::save_dirty();
        $trs->commit();
    }
    
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpMission::GET_MISSION_REWARD:

default:
    break;

} // switch (Response::getOpcode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for mission');

// EOF
