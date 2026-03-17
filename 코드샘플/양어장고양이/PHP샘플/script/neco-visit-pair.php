<?php

define('NECO_VISIT_PAIR_ACTIVE', true);

// 짝지어 등장할 고양이-오브젝트, 짝꿍 고양이
define('NECO_VISIT_PAIR', [
    eNecoId::DDUNG => [
        eObjectType::PIPE_CATTOWER => eNecoId::CASANOVA
    ]
]);
// NECO_VISIT_PAIR의 리버스 인덱스
define('NECO_CHURU_LEAVING_PAIR', [
    eNecoId::CASANOVA => [eObjectType::PIPE_CATTOWER, eNecoId::DDUNG]
]);

// Neco::visit_obj에서 호출할 것
function post_neco_visit_pair_check(int $nid, int $oid, int $now) : void {
    if (!NECO_VISIT_PAIR_ACTIVE) {
        // disabled
        return;
    }

    if (!isset(NECO_VISIT_PAIR[$nid][$oid])) {
        // no paired neco
        return;
    }

    $nid2 = NECO_VISIT_PAIR[$nid][$oid];
    $neco = Neco::get($nid2);
    if (!$neco || !$neco->is_active()) {
        // no neco
        return;
    }

    if (VisitManager::is_neco_free($nid2)) {
        // free -> move map #0
        VisitManager::set_is_neco_free($nid2, false);
        $neco->move_map(0);
        return;
    } else {
        // remove from object
        SpotObject::find_and_remove_neco($now, $nid2);
    }
}

// VisitManager::init 마지막에 호출
function post_free_neco_list_pair_check() : void {
    if (!NECO_VISIT_PAIR_ACTIVE) {
        // disabled
        return;
    }

    foreach (NECO_VISIT_PAIR as $nid1 => $arr) {
        if (VisitManager::is_neco_free($nid1)) {
            // trigger 고양이가 잡혀 있지 않음
            continue;
        }

        remove_neco_on_pair_condition($nid1, $arr);
    }
}

function remove_neco_on_pair_condition(int $nid1, array $list) : void {
    if (!is_array($list)) {
        return;
    }

    foreach ($list as $oid => $nid2) {
        $obj = SpotObject::get($oid);
        if (!$obj) {
            continue;
        }

        $visits = $obj->get_visits();
        foreach ($visits as $slot => $v) {
            if (isset($v['id']) && $nid1 === $v['id']) {
                VisitManager::set_is_neco_free($nid2, false);
            }
        }
    }
}

// EOF
