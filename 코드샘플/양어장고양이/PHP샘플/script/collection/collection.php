<?php

class OpCollection
{
    const REQ_REWARD = 1;

    const COLLECTION_REWARDED = 101;
}

class ResCollection
{
    const OK = 0;
    const REQUIREMENT_NOT_MET = 1;
    const ALREADY_REWARDED = 2;
}

function api_collection_error(int $err, array $data = null) {
    $rs = ['rs' => $err];
    if ($data) {
        $rs = array_merge($rs, $data);
    }
    Response::addApiResponse('', 0, $rs);
    Response::ok();
}

// EOF
