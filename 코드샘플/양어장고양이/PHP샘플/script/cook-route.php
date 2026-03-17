<?php

req_once(SRC_PATH . '/recipe/recipe.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpCook::COOK:
{
    $rid = get_numeric_param('rid');
    $type = get_numeric_param('type', 0);
    $rep = get_numeric_param('rep', 1);

    $rs = ResCook::OK;
    if (ResRecipe::OK != ($rs = check_recipe_material($rid, $type, $rep))) {
        Response::addApiResponse('', 0, ['rs' => $rs, 'rid' => $rid, 'type' => $type, 'rep' => $rep]);
        Response::ok();
    }

    $trans = GameDb::getScopedTransaction();

    //$api = do_craft($rid, $rep, Usage::COOK);
    do_craft($rid, $type, $rep, Usage::COOK);

    $trans->commit();

    Response::ok();
} // case OpCook::COOK:

default:
    break;

} // switch(Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'cook-route.php: invalid op');

// EOF
