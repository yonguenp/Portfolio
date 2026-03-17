<?php

class OpFeed
{
    const FEED = 1;
}

class eResFeed
{
    const OK = 0;
    const LACKS_FOOD = 1;
    const INVALID_ITEM = 2;
}

function try_feed(int $item_id) {
    if (!$item_id) {
        return eResFeed::INVALID_ITEM;
    }

    $inven = User::GetInventory();
    if (1 > $inven->get_item_amount($item_id)) {
        return eResFeed::LACKS_FOOD;
    }

    $inven->update_item_amount($item_id, -1, Usage::INTER_FEED);

    return eResFeed::OK;
}

// EOF
