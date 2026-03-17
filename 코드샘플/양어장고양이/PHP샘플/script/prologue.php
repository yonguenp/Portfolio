<?php

function convert_prologue_restart(int $seq) : int {
    switch ($seq) {
        case ePrologueSeq::START:
        case ePrologueSeq::FISHFARM_BTN_APPEAR:
        case ePrologueSeq::FISHFARM_UI_OPENED:
        case ePrologueSeq::FISHFARM_UI_TAKE_BTN:
            return ePrologueSeq::START;

        case ePrologueSeq::FISHFARM_UI_CLOSED:
        case ePrologueSeq::GILMAK_SILHOUETTE:
        case ePrologueSeq::GILMAK_SILHOUETTE_SCRIPT:
            return ePrologueSeq::FISHFARM_UI_CLOSED;

        case ePrologueSeq::GILMAK_CLIP_PLAY:
        case ePrologueSeq::GILMAK_CLIP_POST_SCRIPT:
        case ePrologueSeq::GILMAK_SILHOUETTE_DISAPPEAR:
        case ePrologueSeq::FEED_BOWL_APPEAR:
        case ePrologueSeq::FEED_CHOOSED:
            return ePrologueSeq::GILMAK_SILHOUETTE_DISAPPEAR;

        case ePrologueSeq::FED:
        case ePrologueSeq::GILMAK_START_MOVING:
        case ePrologueSeq::GILMAK_VISIT:
        case ePrologueSeq::GILMAK_EATING:
        case ePrologueSeq::GILMAK_PROFILE_UI_OPEN:
        case ePrologueSeq::GILMAK_PROFILE_UI_CLOSED:
        case ePrologueSeq::GILMAK_GOES_BACK:
        case ePrologueSeq::FISHTRAP_SCRIPT:
        case ePrologueSeq::FISHTRAP_APPEAR:
        case ePrologueSeq::FISHTRAP_UI_OPEN:
            return ePrologueSeq::FISHTRAP_APPEAR;

        case ePrologueSeq::FISHTRAP_GET_REWARD:
        case ePrologueSeq::COUNTERTOP_HAMBURGER:
        case ePrologueSeq::COUNTERTOP_UI_PROGRESS:
            return ePrologueSeq::COUNTERTOP_HAMBURGER;

        case ePrologueSeq::COUNTERTOP_COOK_DONE:
            return ePrologueSeq::COUNTERTOP_COOK_DONE;

        case ePrologueSeq::FED2:
        case ePrologueSeq::GILMAK_START_MOVING2:
            return ePrologueSeq::FED2;

        case ePrologueSeq::GILMAK_EATING2:
        case ePrologueSeq::GILMAK_EATING2_SCRIPT:
        case ePrologueSeq::GIFT_UI_UNLOCK:
            return ePrologueSeq::GIFT_UI_UNLOCK;
            
        case ePrologueSeq::GIFT_UI_SCRIPT:
        case ePrologueSeq::WORKBENCH_HAMBURGER:
        case ePrologueSeq::WORKBENCH_UI_PROGRESS:
            return ePrologueSeq::WORKBENCH_HAMBURGER;

        case ePrologueSeq::WORKBENCH_DONE_SCRIPT:
        case ePrologueSeq::FISHPOLE_INSTALLED:
        case ePrologueSeq::FISHPOLE_SCRIPT:
        case ePrologueSeq::CATMOM_APPEAR:
            return ePrologueSeq::CATMOM_APPEAR;

        case ePrologueSeq::CATMOM_APPEAR_SCRIPT:
        case ePrologueSeq::CATMOM_CLIP:
        case ePrologueSeq::CATMOM_OBTAIN:
        case ePrologueSeq::PROLOGUE_END:
            return ePrologueSeq::PROLOGUE_END;

        default:
            break;
    }
    return $seq;
}

// EOF
