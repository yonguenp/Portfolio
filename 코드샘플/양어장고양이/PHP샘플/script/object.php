<?php

class ResObject
{
    const OK = 0;
    const NOTHING_TO_RECEIVE = 1;
    const NO_SUCH_OBJECT = 2;
    const INVALID_PARAM = 3;
    const NOT_ON_VISIT = 4;
}

function res_object_err(int $err) {
    Response::mainApiResponse(['rs' => $err]);
    Response::ok();
    exit;
}

function handle_sudden(SpotObject $obj, int $slot) : array {
    $visit_all = $obj->get_visits();
    $visit = $visit_all[$slot];

    $nid = $visit['id'];
    $type = $visit['state'];
    $clip = $visit['clip'];

    $obj->set_sudden_finished($slot);

    $ret = [
        'rs' => ResObject::OK,
        'oid' => $obj->get_id(),
        'slot' => $slot,
        'type' => $type,
        'clip' => $clip
    ];

    // Sudden 보상은 추억 / 포인트 / 잡템
    $rew = [];
    
    if (eSuddenType::TOUCH === $type) {
        $touch_rew = SuddenData::pick_touch_reward($obj->get_id(), $obj->get_level());
        if ($touch_rew) {
            $rew['item'][] = $touch_rew;
        }
    } else if ($clip) {
        $dupe_pt = $visit['churu'] ?
            NECO_MEMORY_DUPE_POINT_PREMIUM :
            NECO_MEMORY_DUPE_POINT_PLAIN;
            
        $rew['memory'] = [[$clip, $dupe_pt]];
    }

    $rew = User::receive_rewards($rew);        
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }

    return $ret;
}

// EOF
