<?php

define('WALK_CLIP_PLAYTIME', 1);
define('WALK_EVENT_PROC', 50);
define('WALK_EVENT_DURATION', 1);

class OpWalk
{
    const START_WALK = 1;
    const NEXT_STEP = 2;
    const EVENT_INTERACT = 3;
    const FINISH_WALK = 4;

    const WALK_STATE_UPDATE = 101;
    const UNLOCKED_AREA_UPDATE = 102;
}

class ResWalk
{
    const OK = 0;
    const NOT_ENOUGH_LEVEL = 1;
    const NEED_TO_WAIT = 2;
    const INTERACT_TIME_NOT_MET = 3;
    const INVALID_STATE = 4;
    const CANNOT_INTERACT = 5;
    const NOT_ENOUGH_ITEM = 6;
    const NOT_ENOUGH_FULLNESS = 7;
}

class WalkEventType
{
    const TOUCH = 1;
    const PLAY = 2;
    const FEED = 3;
    const WATCH = 4;
}

class WalkAreaType
{
    const NONE = 0;
    const YARD = 1;
    const POND = 2;
}

// function get_walk_area_info(int $area) {
//     static $area_info = [];
//     if (!isset($area_info[$area])) {
//         $area_info[$area] = load_walk_area_info($area);
//     }

//     return $area_info[$area];
// }

// function load_walk_area_info(int $area) {
//     $res = DataRedis::get_row('walk_area', $area);
    
//     if (!$res) {
//         Response::error(ResCode::PARAM_ERR, 'there is no walk area #' . $area);
//     }

//     return [
//         'req_level' => intval($res['req_level'])
//     ];
// }

function response_walk_error(int $err, array $arg = []) {
    $arg['rs'] = $err;
    Response::addApiResponse('', 0, $arg);
    Response::ok();
}

function get_walk_clip_duration(int $clip_id) {
    return WALK_CLIP_PLAYTIME;
}

function get_walk_event_duration(int $event_id) {
    return WALK_EVENT_DURATION;
}

function check_interact_timewindow(int $eid, int $itype, $now) {
    $info = EventData::get_event_info($eid);
    if (!$info) {
        // 여기로 오기 전에 확인했을 것인데 이제와서 없어서는 안 됨
        Response::error(ResCode::SERVER_ERROR);
    }

    $str_inter = interact_to_string($itype);
    if (!array_key_exists('interact', $info)
    || !array_key_exists($str_inter, $info['interact'])) {
        // 뭔가 잘못되었어 여기서 나가야 해
        $walk = User::get_walk();
        $walk->finish_walk();
        $walk->save();
        GameDb::commit();

        Response::addApiResponse('walk', OpWalk::WALK_STATE_UPDATE, $walk->get_packet());

        response_walk_error(ResWalk::CANNOT_INTERACT);
    }

    // 200114 : 상호작용 가능 시간대 체크 로직에서 '너무 늦은' 상호작용 제외
    $from_t = $info['interact'][$str_inter][0];
    //$to_t = $from_t + $info['interact'][$str_inter][1];
    // safe margin 1 sec
    $from_t -= 1;
    //$to_t += 1;

    //return ($from_t <= $now && $now <= $to_t);
    return ($from_t <= $now);
}

/**
 * 2020-12-22 renewal
 */

function check_walk_res_array(array $res) {
    if (!isset($res['rs']) || ResWalk::OK !== $res['rs']) {
        Response::addApiResponse('', 0, $res);
        Response::ok();
    }
}

function try_start_action(int $area, int $atype) {
    switch($atype) {
        case ActionType::WALK:
            return try_start_walk($area, $atype);
            break;

        case ActionType::FISH:
            return try_start_fish($area);
            //Response::error(ResCode::SERVER_ERROR, '', new Exception('ActionType::FISH'));
            break;
        case ActionType::FEED:
            return try_start_feed($area);
        default:
        break;
    }
    Response::error(ResCode::PARAM_ERR, 'try_start_walk');
}

function try_start_walk(int $area, int $atype) {
    // skip area check
    $eid = EventData::pick_entry_event($atype, $area);
    if (!$eid) {
        User::get_walk()->finish_walk();
        return ['rs' => ResWalk::OK];
    }

    if (!EventData::is_fullness_met($eid)) {
        return ['rs' => ResWalk::NOT_ENOUGH_FULLNESS];
    }

    User::get_walk()->start_walk($eid, $area, $atype);
    User::get_walk()->save();

    //User::UpdateFullness(-1 * EventData::get_event_fullness_cost($eid), Usage::WALK);

    return ['rs' => ResWalk::OK];
}

function try_start_fish($area) {
    $eid = EventData::pick_fish_event();
    if (!$eid) {
        User::get_walk()->finish_walk();
        return ['rs' => ResWalk::OK];
    }

    $walk = User::get_walk();
    $walk->start_walk($eid, $area, ActionType::FISH);
    $walk->save();

    // start에서 보상 주지 않음
    // $rew = EventData::get_event_reward($eid);
    // if ($rew && !empty($rew)) {
    //     return ['rs' =>ResWalk::OK, 'rew' => $rew];
    // } else {
        return ['rs' =>ResWalk::OK];
    // }
}

function try_start_feed(int $item_id) {
    // item check
    if (0 > User::GetInventory()->get_item_amount($item_id)) {
        return ['rs' => ResWalk::NOT_ENOUGH_ITEM];
    }

    $walk = User::get_walk();
    $eid = EventData::pick_entry_event(ActionType::FEED, $item_id);
    if (!$eid) {
        $walk->finish_walk();
        $walk->save();
        return ['rs' => ResWalk::OK];
    }

    $walk->start_walk($eid, $item_id, ActionType::FEED);
    $walk->save();

    // User::GetInventory()->update_item_amount($item_id, -1, Usage::INTER_FEED);
    // User::UpdateFullness(Data::get_item_fullness($item_id), Usage::INTER_FEED);

    return ['rs' => ResWalk::OK];
}

function try_next_step() {
    $walk = User::get_walk();
    $eid = $walk->get_event_id();

    $ret = ['rs' => ResWalk::OK];
    $rew = EventData::get_event_reward($eid);
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }

    $walk->get_event_seen()->set($eid);

    $hungry = -1 * EventData::get_event_fullness_cost($eid);
    if (0 > $hungry) {
        User::UpdateFullness($hungry, Usage::WALK);
    }

    if (ActionType::FEED == $walk->get_action_type()) {
        $item_id = $walk->get_area();
        User::GetInventory()->update_item_amount($item_id, -1, Usage::INTER_FEED);
        User::UpdateFullness(Data::get_item_fullness($item_id), Usage::INTER_FEED);
    }

    $next = EventData::pick_chaining_event($eid, InteractionType::FINISH);
    if (!$next) {
        $walk->finish_walk();
        $walk->save();
        return $ret;
    }

    if (!EventData::is_fullness_met($next)) {
        return ['rs' => ResWalk::NOT_ENOUGH_FULLNESS];
    }

    $walk->go_to_event($next);
    $walk->save();

    //User::UpdateFullness(-1 * EventData::get_event_fullness_cost($next), Usage::WALK);

    return $ret;
}

function try_interact(int $type) {
    $walk = User::get_walk();
    $eid = $walk->get_event_id();

    if (!check_interact_timewindow($eid, $type,
        Response::getTs() - $walk->get_event_start())) {
        response_walk_error(ResWalk::INTERACT_TIME_NOT_MET);
    }

    $ret = ['rs' => ResWalk::OK, 'type' => $type];
    $rew = EventData::get_event_reward($eid);
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }

    $walk->get_event_seen()->set($eid);

    $success = false;
    $next = EventData::pick_chaining_event($eid, $type, true);
    if ($next) {
        if (EventData::is_fullness_met($next)) {
            $success = true;
        }
    }
    $ret['success'] = intval($success);

    if (!$success) {
        $next = EventData::pick_chaining_event($eid, $type, false);
    }

    $hungry = -1 * EventData::get_event_fullness_cost($eid);
    if (0 > $hungry) {
        User::UpdateFullness($hungry, $usage);
    }

    if (!$next) {
        $walk->finish_walk();
        $walk->save();        
        return $ret;
    }
    
    $walk->go_to_event($next);
    $walk->save();

    if ($success) {
        $usage = Usage::WALK;
        switch($type) {
        case InteractionType::TOUCH:
            $usage = Usage::INTER_TOUCH;
            break;
        case InteractionType::FEED:
            $usage = Usage::INTER_FEED;
            break;
        case InteractionType::PLAY:
            $usage = Usage::INTER_PLAY;
            break;
        case InteractionType::FISH:
            break;
        default:
            break;
        }
    }

    return $ret;
}

// EOF
