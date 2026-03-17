<?php

req_once(SRC_PATH . 'adventure/adventure.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpAdventure::START_STAGE:
{
    $stageid = get_numeric_param('stage');
    $adv = User::get_adventure();

    // checks adventure state && location opened
    if (ResAdventure::OK != ($rs = $adv->can_start_stage($stageid))) {
        Response::api_error($rs);
    }

    // go
    $res = $adv->start_stage($stageid);

    Response::mainApiResponse($res);
    Response::ok();
} break;// case OpAdventure::START_STAGE:

case OpAdventure::START_AUTO:
{
    $str_list = get_param('stages');

    // 자동 탐험의 재생목록 parse
    $str_list = explode(',', $str_list);
    $auto_list = [];
    foreach((array)$str_list as $stage_id) {
        if ($stage_id) {
            $auto_list[] = intval($stage_id);
        }
    }

    if (empty($auto_list)) {
        Response::error(ResCode::PARAM_ERR);
    }

    $adv = User::get_adventure();

    // 재생 목록의 스테이지가 [모두 존재하며] / [개방된 단일한 지역에 속하며] / [자동플레이 가능한 상태인지] 확인
    if (ResAdventure::OK !== ($rs = $adv->check_auto_list_validity($auto_list))) {
        Response::api_error($rs);
    }

    // 자동 탐험은 스테이지 전체의 포만감 소모를 사전에 확인해야 함
    $stage = Adventure::get_stage_info($auto_list[0]);
    if ($stage->fullness > User::GetFullness()) {
        Response::api_error(ResAdventure::NOT_ENOUGH_FULLNESS, ['stage' => $stageid]);
    }

    // 시작
    $res = $adv->start_stage($auto_list[0], $auto_list);
    $res['stages'] = $str_list;

    Response::mainApiResponse($res);
    Response::ok();
} break;// case OpAdventure::START_AUTO:


case OpAdventure::INTERACT:
case OpAdventure::NEXT_STEP:
{
    /**
     * [상호작용 행동을 통해 / 단순 재생 종료로] 다음 클립으로 이동
     */
    
    // 상호작용이 없다면 finish type
    $itype = get_numeric_param('type', InteractionType::FINISH);
    $score = get_numeric_param('score', 0);
    $transaction = GameDb::getScopedTransaction();

    $res = try_next_adventure_step($itype, $score);

    if (isset($res['rew'])) {
        $res['rew'] = User::receive_rewards($res['rew'], Usage::WALK);
    } else {
        User::SaveBaseInfo();
    }

    Response::mainApiResponse($res);

    $transaction->commit();
    Response::ok();
} break; // case OpAdventure::INTERACT:

// case OpAdventure::NEXT_STEP:
// {
//     $score = get_numeric_param('score', 0);
//     $transaction = GameDb::getScopedTransaction();

//     $res = try_next_adventure_step(InteractionType::FINISH, $score);

//     if (isset($res['rew'])) {
//         $res['rew'] = User::receive_rewards($res['rew'], Usage::WALK);
//     } else {
//         User::SaveBaseInfo();
//     }

//     Response::mainApiResponse($res);

//     $transaction->commit();
//     Response::ok();
// } break; // case OpAdventure::NEXT_STEP:

case OpAdventure::ABORT_STAGE:
{
    $adv = User::get_adventure();
    if (eStageState::PLAYING === $adv->get_state()) {
        $adv->abort_stage();
    }

    Response::mainApiResponse(['rs' => ResAdventure::OK]);
    Response::ok();
} break; // case OpAdventure::ABORT_STAGE:

case OpAdventure::QUEUE_ABORT_STAGE:
{
    $adv = User::get_adventure();
    if (eStageState::PLAYING !== $adv->get_state() || !$adv->is_auto()) {
        Response::api_error(ResAdventure::INVALID_STATE);
    }

    $val = get_numeric_param('val');

    $adv->queue_abort_stage($val);
    Response::mainApiResponse(['rs' => ResAdventure::OK]);
    Response::ok();
} break; // case OpAdventure::QUEUE_ABORT_STAGE:

case OpAdventure::STAGE_REWARD:
{
    $stage_id = get_numeric_param('stage');
    $stage = Adventure::get_stage_info($stage_id);
    if (!$stage) {
        Response::api_error(ResAdventure::INVALID_STAGE);
    }

    $adv = User::get_adventure();
    if ($adv->get_stage_progress($stage_id) < $stage->max_star ||
        $adv->get_stage_reward($stage_id)) {
        Response::api_error(ResAdventure::INVALID_STATE);
    }

    $trans = GameDb::getScopedTransaction();
    $res = $adv->receive_stage_reward($stage_id);
    if (ResAdventure::OK !== $res['rs']) {
        Response::api_error($res['rs'], $res);
    }

    if (isset($res['rew'])) {
        $res['rew'] = User::receive_rewards($res['rew'], Usage::ADVENTURE_STAGE);
    }

    Response::mainApiResponse($res);
    $trans->commit();
    Response::ok();

} break; // case OpAdventure::STAGE_REWARD:

case OpAdventure::LOCATION_REWARD:
{
    $location_id = get_numeric_param('location');
    $reward_type = get_numeric_param('type');
    if (eLocationRewardType::QUATER > $reward_type ||
        eLocationRewardType::FULL < $reward_type) {
        Response::api_error(ResAdventure::INVALID_LOCATION, ['location' => $location_id]);
    }

    $trans = GameDb::getScopedTransaction();

    $adv = User::get_adventure();
    $res = $adv->receive_location_reward($location_id, $reward_type);
    if (ResAdventure::OK === $res['rs']) {
        if (isset($res['rew'])) {
            $res['rew'] = User::receive_rewards($res['rew'], Usage::ADVENTURE_LOCATION);
        }
        $trans->commit();
    } else {
        GameDb::rollback();
        Response::api_error($res['rs'], $res);
    }

    Response::mainApiResponse($res);
    Response::ok();
} break; // case OpAdventure::LOCATION_REWARD:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for adventure');

// EOF
