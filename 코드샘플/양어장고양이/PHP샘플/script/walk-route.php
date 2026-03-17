<?php

req_once(SRC_PATH . '/walk/walk.php');

validate_session_token();

$walk = User::get_walk();

$is_auto = get_numeric_param('auto', 0);
$walk->set_is_auto($is_auto);

switch(Response::getOpCode()) {

    case OpWalk::START_WALK:
    {        
        // expiration check
        $res = $walk->check_expiration();
        check_walk_res_array($res);

        $area = get_numeric_param('area');
        $atype = get_numeric_param('atype', ActionType::WALK);
        if (WalkAreaType::POND === $area && ActionType::WALK === $atype) {
            $atype = ActionType::FISH;
        }

        $res = try_start_action($area, $atype);
        check_walk_res_array($res);
        
        Response::addApiResponse('', 0, $res);
        if (ResWalk::OK === $res['rs']) {
            $walk->write_state_response();
        }

        User::SaveBaseInfo();

        Response::ok();
    } break; // case OpWalk::START_WALK:

    case OpWalk::NEXT_STEP:
    {
        // expiration check
        $res = $walk->check_expiration();
        check_walk_res_array($res);

        GameDb::startTransaction();

        // 상태 변경하고 api response 얻음
        $res = try_next_step();

        // 보상이 있다면 지급
        if (isset($res['rew'])) {
            $res['rew'] = User::receive_rewards($res['rew'], Usage::WALK);
        }

        Response::addApiResponse('', 0, $res);

        // 변경 성공시 상태 갱신
        if (ResWalk::OK === $res['rs']) {
            $walk->write_state_response();
        }

        GameDb::commit();
        Response::ok();
    } break; // case OpWalk:::

    case OpWalk::EVENT_INTERACT:
    {
        $type = get_numeric_param('type');
        if (!$type) {
            Response::error(PARAM_ERR);
        }

        if (WalkState::EVENT_PLAYING !== $walk->get_state()) {
            response_walk_error(ResWalk::INVALID_STATE);
        }

        GameDb::startTransaction();
        
        // 상태 변경하고 api response 얻음
        $res = try_interact($type);

        // 상태가 실제 변경되었다면 새 상태를 알림
        if (ResWalk::OK === $res['rs']) {
            $walk->write_state_response();
        }

        // 보상 있으면 전달
        if (isset($res['rew'])) {
            $res['rew'] = User::receive_rewards($res['rew'], Usage::WALK);
        } else {
            User::SaveBaseInfo();
        }

        Response::addApiResponse('', 0, $res);

        GameDb::commit();
        Response::ok();
    } break; // case OpWalk:::
    
    case OpWalk::FINISH_WALK:
    {
        if (WalkState::WALKING !== $walk->get_state()) {
            response_walk_error(ResWalk::INVALID_STATE);
        }

        $walk->finish_walk();

        Response::addApiResponse('', 0, ['rs' => ResWalk::OK]);
        $walk->write_state_response();

        Response::ok();
    } break; // case OpWalk::FINISH_WALK:

    default:
        break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'walk opcode not captured. #' . Response::getOpCode());

// EOF