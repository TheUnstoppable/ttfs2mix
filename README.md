# ttfs2mix
[![Jenkins Build](https://img.shields.io/jenkins/build?jobUrl=https%3A%2F%2Fci.unstoppable.work%2Fjob%2FRenegade%2520Tools%2Fjob%2Fttfs2mix%2F)](https://ci.unstoppable.work/job/Renegade%20Tools/job/ttfs2mix/)
[![Buy me a Coffee](https://img.shields.io/badge/buy%20me%20a%20coffee-yellow)](https://buymeacoffee.com/theunstoppable)

## Overview
Utility to convert TTFS packages to MIX packages.

## Usage
- `convert <package id/name>`: Converts first occurence of TTFS package to MIX file and saves into data folder.
- `convertall`: Converts first occurence of TTFS package to MIX file and saves into data folder.
- `multiconvert <package id/name>`: Converts all matching TTFS packages to MIX files and saves into data folder.
- `download <package id/name> <url> [--count <num>]`: Finds and downloads first occurence of TTFS package from a remote repository to MIX file and saves into data folder.
- `downloadall <url> [--count <num>]`: Finds and downloads all TTFS packages from a remote repository to MIX files and saves all into data folder.
- `multidownload <package id/name> <url> [--count <num>]`: Finds and downloads all matching TTFS packages from a remote repository to MIX files and saves into data folder.

## Disclaimer
**To tool users:** Contributors to this project are not responsible for any assets or packages that are stolen, rehosted, or modified without permission from the respective owner. By using this tool, you accept full liability for any issues that may arise from converting assets and packages without the owner's permission.  
**To package owners:** Contributors to this project will not be liable or responsible for any packages converted using this tool.  
