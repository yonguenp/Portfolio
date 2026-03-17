import React from "react";

export default function ClaimManual(props) {
  const { closePopup } = props;

  return (
    <div>
        <div className="bg"></div>
        <div className="container">
            <div className="textBox ">
                <div className="font-h6">Daily vault</div>
                <div className="font-b3-r gray-500 text-left">
                    The daily vault holds up to 5,000 Magnite.
                    <br />
                    The vault is refilled daily at 00:00 (UTC+9).
                    <br />
                    MTDZ NFT holders can claim Magnite on their own conditions.
                    <br />
                    Once the vault is depleted, claims cannot be made until the next refresh time.
                    <br />
                    Decimal points are omitted.
                    <br />
                    Claiming more than the remaining Magnite is not allowed.
                </div>
            </div>
            <br />
            <div className="singleBtn font-b3-b white" onClick={closePopup}>
                Close
            </div>
        </div>
    </div>
  );
}
