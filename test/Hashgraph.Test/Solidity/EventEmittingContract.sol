pragma solidity 0.5.3;

contract EventEmittingContract {
    
    address payable owner;
    
    event HbarsSent(address sent_to, uint amount);
    
    constructor() public payable {
        owner = msg.sender;
    }
    
    function get_balance() public view returns (uint) {
        return address(this).balance;
    }

    function send_to(address payable to_) public requireOwner {
        uint amount = address(this).balance;
        to_.transfer(amount);
        emit HbarsSent(to_,amount);
    }

    function() external payable { }
    
    function kill() public requireOwner { 
        selfdestruct(msg.sender); 
    }
    
    modifier requireOwner {
        require(msg.sender == owner, "Only contract owner can call this function.");
        _;
    }
}