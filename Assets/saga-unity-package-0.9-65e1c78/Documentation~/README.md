# Saga Unity Plugin

## Overview

This package abstracts many of the low-level transactions needed to
deal with a Saga instance in a performant and ergonomic fashion. 

In particular, Users, Bots, and the Global singleton are all first-class
objects accessed and modified via the `User`, `Bot`, and `Globals` classes, respectively. 

These classes, once fetched from the service with `SAGA.USER(...)`, `SAGA.BOT(...)`, 
and `SAGA.GLOBALS(...)`, are automatically synced in the background for the 
runtime of the game or until `Unsubscribe()` is called on the
`User`, `Bot`, or `Globals`object.

This means that whenever you access a property, the transaction is instant and lightweight,
and the property is up-to-date.

## Installation

To install the package, simply move the unzipped directory into the
packages folder.

## Usage
 
Many of the functions included in this package are async. To use these
 in your `MonoBehaviour`, make sure to prefix lifetime
methods with `async`:

```c# 
async void Start() { ... } 
async void OnDisable() { ...}
```

### Initialization

```c#
await SAGA.InitAsync("https://your_saga_instance", "username", "password");
```

### User Handling

To get a `User` object and subscribe to a user, do the following:
```c#
User testUser = await SAGA.USER("username");
```

If you are not sure if the user exists, make sure to surround in a try/catch
block.

In the future, when you call `SAGA.USER("username")`, you will retrieve the
synced `User` object, so checking a user's properties over time is extremely
low-cost.

### Bot Handling

To get a `Bot` object and subscribe to a bot, do the following:
```c#
Bot testBot = await SAGA.BOT("name");
```

If you are not sure if the user exists, make sure to surround in a try/catch
block.

In the future, when you call `SAGA.BOT("name")`, you will retrieve the
synced `Bot` object, so checking a bot's properties over time is extremely
low-cost.

### Global Handling

To get a `Globals` object and subscribe to global properties, do the following:
```c#
Globals globals = await SAGA.GLOBALS();
```

In the future, when you call `SAGA.GLOBALS()`, you will retrieve the
synced `Globals` object, so checking global properties over time is extremely
low-cost.

### Property Handling

Once you have retrieved a `User`, `Bot`, or `Globals` object, accessing and
modifying properties is simple!

To get a property and cast it to a type: 
```c#
string value = user["prop_name"].Get<string>();
```

To set a property:
```c#
await user["prop_name"].AddAsync(object value);
```

To handle a property change event:
```c#
user.On["prop_name"] += (property) => {
    var num = property.Get<int>();
};
```

### Message Handling

To send messages from a user to a bot:

```c#
Bot bot = SAGA.BOT(...);
await user.SendMessageAsync(bot, "Hello!");
```
Or
```c#
string botId = "5db0acbc46d06a0089c1f06b";
await user.SendMessageAsync(botId, "Hello!");
```
To handle received messages for both users and bots:
```c#
user.OnMessage += (message) => {
    Debug.Log("Received message: " + message.message);
}
```

### Error Handling

Saga-specific errors are derived from type `SagaException`. Most of the `async` functions will
throw these, so be sure to catch and handle them.

There are also more specific errors derived from `SagaException`:

- `UninitializedException`
- `BadRequestException`
- `NotFoundException`
- `UnprocessableException`
- `UnauthorizedException`
- `ServerException`

```c#
try {
    await SAGA.InitAsync(...);
catch(UnauthorizedException e) {
    // Handle
}
```

