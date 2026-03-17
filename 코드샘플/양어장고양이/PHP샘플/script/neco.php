<?php

function res_neco_error(int $e) {
    Response::mainApiResponse(['rs' => $e]);
    Response::ok();
    exit;
}

function try_tame_neco(int $id) : void {
    $neco = Neco::get($id);
    if ($neco) {
        if ($neco->is_active()) {
            return;
        }

        $neco->set_state(eNecoState::NEW);
    } else {
        $neco = Neco::create($id, eNecoState::NEW);

        if (!$neco) {
            Response::error(ResCode::SERVER_ERROR);
        }
    }

    $neco->save_sql();
    Response::addApiResponse('neco', OpNeco::NEW_NECO, ['new' => $neco->get_packet()]);
}

function remove_neco_from_object(int $nid) {
    $objects = SpotObject::get_all();
    foreach ($objects as $id => $o) {
        $v = $o->get_visits();
        foreach ($v as $slot => $arr) {
            if ($nid == $arr['id']) {
                $o->remove_neco(Response::getTs(), $slot);

                // override
                on_client_removed_neco($nid, $o);

                return;
            }
        }
    }
}

function on_client_removed_neco(int $nid, SpotObject $obj) {
    if (eNecoId::GILMAK === $nid
        && eObjectType::FEED_BOWL === $obj->get_id()
        && eItemId::FISH_FEED === $obj->feed_id) {
        $obj->feed_tick_expire = Response::getTs();
        $obj->feed_tick_expire_post = Response::getTs() + 10;
    }
}

function parse_map_param(string $param) : array {
    $ret = [];

    $exp = explode(',', $param);
    foreach ($exp as $frag) {
        $tmp = explode(':', $frag);
        if (2 === count($tmp)) {
            $ret[$tmp[0]] = intval($tmp[1]);
        }
    }

    return $ret;
}

function do_move_neco(array $order) : array {
    $map = [];

    foreach ($order as $id => $map_id) {
        //error_log('trying to move ' . $id . ' to ' . $map_id);
        if ($map_id && !SpotObject::is_map_feed_remain($map_id)) {
            //error_log('no feed given');
            continue;
        }
        $nid = intval($id);
        $neco = Neco::get($nid);
        if ($neco && $neco->is_active() &&
                VisitManager::is_neco_free($nid) &&
                $map_id !== $neco->get_map()) {
            $neco->move_map($map_id);
            $map[] = [$nid, $map_id];
            //error_log('migrated neco');
        } else {
            // if (!$neco) {
            //     error_log('no neco');
            // } else if (!VisitManager::is_neco_free($nid)) {
            //     error_log('neco not free');
            // } else if ($map_id === $neco->get_map()) {
            //     error_log('already on map');
            // }
        }
    }

    if (empty($map)) {
        return ['rs' => ResNeco::NO_CHANGE];
    } else {
        return ['rs' => ResNeco::OK];
    }
}

function take_greeting_gift(int $id) : array {
    $ret = ['id' => $id];

    // neco
    $neco = Neco::get($id);
    if (!$neco || !$neco->is_active()) {
        $ret['rs'] = ResNeco::NO_SUCH_NECO;
        return $ret;    
    } else if (!$neco->is_greeting_up()) {
        $ret['rs'] = ResNeco::ALREADY_GIVEN;
        return $ret;
    }

    $neco->set_greeted();
    $neco->save_sql();
    $pkt = Neco::get_dirty_packet();
    if (!empty($pkt)) {
        Response::addApiResponse(
            'neco',
            OpNeco::STATE_UPDATE,
            ['neco' => $pkt]
        );
    }
    
    $rew = neco_greeting_reward($id);
    if ($rew && !empty($rew)) {
        $ret['rew'] = User::receive_rewards($rew);
    }

    $ret['rs'] = ResNeco::OK;
    return $ret;
}

function neco_greeting_reward(int $id) : array {
    $info = NecoData::get_neco_data($id);
    if ($info) {
        return ['gold' => $info['greeting']];
    } else {
        return [];
    }
}

function find_churu_target(int $id) : array {
    $ret = [];
    
    $pool = ObjectData::find_churu_target_from_data($id);
    if (empty($pool)) {
        return $ret;
    }

    // pick a object
    $idx = array_rand($pool);

    return $pool[$idx];
}

// EOF
