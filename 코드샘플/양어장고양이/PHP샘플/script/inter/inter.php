<?php

// 'inter' api용 src

define('IDLE_MIN_SEC', 3);
define('IDLE_MAX_SEC', 4);

class OpInter
{
    const TOUCH = 1;
    const PLAY = 2;
    const FEED = 3;
    const IDLE = 4;

    // response only
    const NEW_TOUCH = 101;
    const NEW_PLAY = 102;
    const IDLE_STATE = 103;
}

class ResInter
{
    const OK = 0;
    const NOT_LEARNT = 1;
    const IDLE_TOO_SHORT = 2;
}

// EOF
