// SPDX-License-Identifier: Apache-2.0
pragma solidity >=0.5.0 <0.9.0;
pragma experimental ABIEncoderV2;

contract TransferToken {

    address constant precompileAddress = address(0x167);
    bytes4 constant transferTokenFn = bytes4(keccak256("transferToken(address,address,address,int64)"));

    event BeforeTransfer(address token, address sender, address receiver, int64 amount);
    event AfterTransfer(address token, address sender, address receiver, int64 amount, int responseCode);

    function transferToken(address token, address sender, address receiver, int64 amount) public returns (int responseCode)
    {
        emit BeforeTransfer(token, sender, receiver, amount);
        (bool success, bytes memory result) = precompileAddress.call(abi.encodeWithSelector(transferTokenFn, token, sender, receiver, amount));
        responseCode = success ? abi.decode(result, (int32)) : int32(0);
        emit AfterTransfer(token, sender, receiver, amount, responseCode);        
    }
}