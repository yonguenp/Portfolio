<?php

class OpPost
{
    const LIST = 1;
    const GET_ONE = 2;
    const GET_ALL = 3;

    const LAST_POST = 101;
}

class eResPost
{
    const OK = 0;
    const NO_SUCH_POST = 1;
    const NO_ATTACHMENT = 2;
    const EXPIRED = 3;
    const REWARDED = 4;
    const SERVER_ERROR = 5;
}

class ePostState
{
    const NEW = 0;
    const READ = 1;
    const REWARDED = 2;
    const EXPIRED = 3;
    const DELETED = 4;
}

// EOF
