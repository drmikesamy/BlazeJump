# BlazeJump - A Blazor and C# boilerplate for Nostr

BlazeJump is a bare bones Nostr client intended to be a boilerplate project for C# Blazor Nostr projects.

## Features

* In-browser SQLite DB Context to manage user and message data
* Strong types for:
  * Events
  * Messages
  * Users
  * Filters
  * Event Tags
* Enums for Event Kind, Message Types, Relay Connection Status, and Tags
* JSON converters to serialise and deserialise Nostr protocol json into Plain Old Class Objects
* BlazeJump can generate RSA keys using SubtleCrypto, and it has support for the Nos2x browser plugin
* Automapper support for easy mapping
* Has separation of concerns with separate services for connections, crypto, database, messages and user profiles

# Setup
Open up with Visual studio 2022 and run
