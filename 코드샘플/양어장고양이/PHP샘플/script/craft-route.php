<?php

req_once(SRC_PATH . '/recipe/recipe.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpCraft::CRAFT:
{
    $rid = get_numeric_param('rid');
    $type = 0;
    $rep = get_numeric_param('rep', 1);

    $rs = ResCraft::OK;
    if (ResRecipe::OK != ($rs = check_recipe_material($rid, $type, $rep))) {
        Response::addApiResponse('', 0, ['rs' => $rs, 'rid' => $rid]);
        Response::ok();
    }

    $st = GameDb::getScopedTransaction();

    do_craft($rid, $type, $rep, Usage::CRAFT);

    $st->commit();

    Response::ok();
} // case OpCook::COOK:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'craft-route.php: invalid op');

// EOF
