// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract VirtualLand2 is ERC721, Ownable {
    struct Land {
        uint256 x;
        uint256 y;
        uint256 width;
        uint256 height;
        address landOwner;
    }

    uint256 public nextTokenId;
    uint256 public gridWidth;
    uint256 public gridHeight;
    mapping(uint256 => Land) public lands;

    constructor(uint256 _gridWidth, uint256 _gridHeight) 
        ERC721("VirtualLand2", "VLAND") 
        Ownable(msg.sender) 
    {
        gridWidth = _gridWidth;
        gridHeight = _gridHeight;
    }

    function buyLand(uint256 x, uint256 y, uint256 width, uint256 height) public payable {
        require(x + width <= gridWidth, "Land exceeds grid width");
        require(y + height <= gridHeight, "Land exceeds grid height");
        require(!isOverlapping(x, y, width, height), "Land overlaps with an existing one");

        uint256 tokenId = nextTokenId;
        nextTokenId++;

        lands[tokenId] = Land(x, y, width, height,msg.sender);
        _safeMint(msg.sender, tokenId);
    }

    function isOverlapping(uint256 x, uint256 y, uint256 width, uint256 height) internal view returns (bool) {
        for (uint256 i = 0; i < nextTokenId; i++) {
            Land storage land = lands[i];
            if (x < land.x + land.width && x + width > land.x &&
                y < land.y + land.height && y + height > land.y) {
                return true;
            }
        }
        return false;
    }

    function getLand(uint256 tokenId) public view returns (uint256, uint256, uint256, uint256) {
        Land storage land = lands[tokenId];
        return (land.x, land.y, land.width, land.height);
    }

    function getAllLands() public view returns (uint256[] memory, uint256[] memory, uint256[] memory, uint256[] memory, address[] memory, uint256[] memory) {
        uint256[] memory tokenIds = new uint256[](nextTokenId);
        uint256[] memory xs = new uint256[](nextTokenId);
        uint256[] memory ys = new uint256[](nextTokenId);
        uint256[] memory widths = new uint256[](nextTokenId);
        uint256[] memory heights = new uint256[](nextTokenId);
        address[] memory owners = new address[](nextTokenId);

        for (uint256 i = 0; i < nextTokenId; i++) {
            Land storage land = lands[i];
            tokenIds[i] = i;
            xs[i] = land.x;
            ys[i] = land.y;
            widths[i] = land.width;
            heights[i] = land.height;
            owners[i] = land.landOwner;
        }

        return (xs, ys, widths, heights, owners, tokenIds);
    }

    function transferLand(uint256 tokenId, address newOwner) public {
        require(msg.sender == ownerOf(tokenId), "Only land owner can transfer the land");
        require(newOwner != address(0), "New owner cannot be the zero address");

        // Update the land ownership in the mapping
        lands[tokenId].landOwner = newOwner;

        // Transfer the token using ERC721's transfer function
        _transfer(msg.sender, newOwner, tokenId);
    }
    
}