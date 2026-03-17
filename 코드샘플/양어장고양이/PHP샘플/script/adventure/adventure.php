<?php

class OpAdventure
{
    const START_STAGE               = 1;
    const START_AUTO                = 2;
    const INTERACT                  = 3;
    const NEXT_STEP                 = 4;
    const ABORT_STAGE               = 5;
    const QUEUE_ABORT_STAGE         = 6;
    const STAGE_REWARD              = 7;
    const LOCATION_REWARD           = 8;
    
    const ADVENTURE_STATE_UPDATE    = 101;
    const STAGE_PROGRESS_UPDATE     = 102;
    const LOCATION_UPDATE           = 103;
}

class ResAdventure
{
    const OK = 0;
    const NOT_ENOUGH_FULLNESS   = 1; // 포만감 부족
    const LOCATION_LOCKED       = 2; // 해당 지역 미개방
    const CANNOT_INTERACT       = 3; // 현재 클립에 상호작용 불가
    const NEED_TO_WAIT          = 4; // 너무 빨리 다음 클립으로 이동 시도
    const INVALID_STATE         = 5; // 현재 탐험 상태와 맞지 않는 요청
    const INVALID_STAGE         = 6;
    const DID_NOT_COMPLETE      = 7;
    const INVALID_LOCATION      = 8;
    const ALREADY_REWARDED      = 9;
    const ITEM_REQUIREMENT_NOT_MET = 10;
}

class eStageState
{
    const NONE = 0;
    const PLAYING = 1;
    const DONE = 2;
}

class eLocationRewardType
{
  const NONE = 0;
  const QUATER = 1;
  const HALF = 2;
  const THREE_QUATER = 3;
  const FULL = 4;
}

/**
 * 유저 반응에 따라 결정되는 다음 클립으로 이동
 *
 * @param integer $itype
 * @param integer $param
 * @return void
 */
function try_next_adventure_step(int $itype = InteractionType::FINISH, int $param = 0) {
    $adv = User::get_adventure();
    // 기획데이터
    if (eStageState::PLAYING !== $adv->get_state()) {
        Response::api_error(ResAdventure::INVALID_STATE);
    }
    // 쿨다운
    if (Response::getTs() < $adv->get_expiration()) {
        Response::api_error(ResAdventure::NEED_TO_WAIT);
    }

    // 현재 재생중 클립
    $clip_id = $adv->get_clip_id();
    $clip = EventData::get_event_info($clip_id);
    if (!$clip) {
        Response::error(ResCode::SERVER_ERROR);
    }

    $ret = ['rs' => ResAdventure::OK];
    
    // 다음 단계 분기 변수. 성공 or 실패
    if ($adv->is_auto()) {
        // 자동 탐험은 언제나 성공으로
        $success = true;
    } else {
        // 성공 조건 충족 여부 확인
        $success = check_if_success($clip_id, $itype, $param);
    }

    // 이전 클립 종료하며 해당 보상 지급 / 소요 포만도 차감
    $rew = EventData::get_event_reward($clip_id);
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }
    $hungry = EventData::get_event_fullness_cost($clip_id);
    if ($hungry) {
        User::UpdateFullness(-1 * $hungry, Usage::WALK);
    }

    // 별점은 성공시에만 지급함
    if ($success) {
        if ($clip['star']) {
            $adv->obtain_stage_star();
        } else if (isset($clip['stage'])){
            $adv->on_stage_progress_update($clip['stage'], 0);
        }
    }

    // 변수에 맞는 다음 클립 확인
    $next_id = pick_chaining_event($clip_id, $adv->is_auto(), $itype, $success);

    if ($next_id) {
        // 다음 클립이 있다면 이동
        $adv->set_clip($next_id);
    } else if ($adv->is_auto()) {
        // 자동재생은 스테이지 끝나면 다음 스테이지
        $adv->try_next_stage();
    } else {
        // 스테이지 종료
        $adv->finish_stage();
    }

    return $ret;
}

function check_if_success(int $clip_id, int $itype, int $param) {
    $info = EventData::get_event_info($clip_id);
    if (!$info) {
        Response::error(ResCode::SERVER_ERROR);
    }

    $success = true;
    if (InteractionType::FINISH !== $itype) {
        $success = check_event_interaction($clip_id, $itype);
    } else {
        $success = check_event_score($clip_id, $itype, $param);
    }
    return $success;
}

function check_event_interaction(int $clip_id, int $itype) {
    $info = EventData::get_event_info($clip_id);
    $str_itype = interact_to_string($itype);
    if (isset($info['interact']) || isset($info['interact'][$str_itype])) {
        if (!isset($info['next'][$str_itype])) {
            return false;
        }
        $success_id = $info['next'][$str_itype][0];
        $on_success = EventData::get_event_info($success_id);
        if (!$on_success) {
            return false;
        }

        if (User::GetFullness() < $on_success['fullness']) {
            return false;
        }

        return true;
    } else {
        return false;
    }
}

function check_event_score(int $clip_id, int $itype, int $param) {
    $info = EventData::get_event_info($clip_id);
    $success_cond = $info['success'];
    
    foreach((array)$success_cond as $k => $v) {
        // touch
        if ('touch_cnt_over' === $k && $v > $param) {
            return false;
        }
    }
    return true;
}

/**
 * 재생중인 클립에 이어질 다음 클립을 검색
 * 1. [상호작용 성공 클립] ($success == true일 때)
 * 2. 상호작용 실패 클립
 * 3. finish 클립
 * 4. 0
 * 순서로 시도
 *
 * @param integer $clip_id
 * @param boolean $is_auto
 * @param integer $itype
 * @param boolean $success
 * @return void
 */
function pick_chaining_event(int $clip_id, bool $is_auto, int $itype, bool $success) {
    $clip = EventData::get_event_info($clip_id);
    if (!$clip) {
        return 0;
    }

    // 자동 -> 단순 finish 요청시에도 상호작용 성공 우선
    $str_itype = interact_to_string($itype);
    if ($is_auto && InteractionType::FINISH === $itype) {
        foreach($clip['next'] as $k => $v) {
            if ($k !== $str_itype) {
                $str_itype = $k;
                break;
            }
        }
    } else if (!isset($clip['next'][$str_itype])) {
        $str_itype = interact_to_string(InteractionType::FINISH);
    }
    
    $queue = [];
    // 해당 상호작용 성공 / 실패
    if (isset($clip['next'][$str_itype])) {
        if ($success) {
            $queue[] = $clip['next'][$str_itype][0];
        }
        if (1 < count($clip['next'][$str_itype])) {
            $queue[] = $clip['next'][$str_itype][1];
        }
    }
    // 종료 클립
    if (isset($clip['next'][interact_to_string(InteractionType::FINISH)])) {
        $queue[] = $clip['next'][interact_to_string(InteractionType::FINISH)][0];
    }

    // 순서대로 검색하여 첫 가능한 클립 번호 반환
    for($i = 0; count($queue) > $i; ++$i) {
        if (check_clip_available($queue[$i])) {
            return $queue[$i];
        }
    }

    // 찾지 못함
    return 0;
}

function check_clip_available(int $clip_id) {
    return EventData::is_condition_item_met($clip_id)
        && check_clip_fullness_cost($clip_id);
}

function check_clip_fullness_cost(int $clip_id) {
    $clip = EventData::get_event_info($clip_id);
    if ($clip = EventData::get_event_info($clip_id)) {
        return User::GetFullness() >= $clip['fullness'];
    }
    return false;
}

// EOF
