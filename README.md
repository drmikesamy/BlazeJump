# BlazeJump - A Blazor and C# Nostr Client

BlazeJump is a bare bones Nostr client written in C#. It currently supports Web and Android platforms, where the Android app is a Nostr client that stores your keypair in Android Secure Storage, and the web client uses NIP46 Nostr Connect protocol to use the Android device as a signer.

## Features

* In-browser SQLite DB Context to manage user and message data
* In-browser Secp256k1 C library (from [bitcoin-core/](https://github.com/bitcoin-core/secp256k1) for communication with a signer phone.
* In-browser Tiny-AES
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
