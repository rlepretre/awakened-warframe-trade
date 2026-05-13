# General

Check other related documentation as well:

[Data Models](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21)

[Websockets](https://www.notion.so/Websockets-1d8515beb0be806a87e9e0fc71aad9aa?pvs=21)

[OAuth 2.0](https://www.notion.so/OAuth-2-0-04e1e72398db4cf8ae1b7b1bae4abcc1?pvs=21)

## Version

Documentation describes: **`v0.23.0`**

## Base URLs

Api base path: `https://api.warframe.market/v2/`

Some fields could be a path to item icon, or user avatar, or profile background, like: `items/images/en/dual_rounds.304395bed5a40d76ddb9a62c76736d94.png`
base path for all kind of these things is: `https://warframe.market/static/assets/`

## **Rate limits**

3 requests per second, for now, probably i’ll rise it later.

## Global Parameters

For each query, you can or must add these parameters to get the results you are looking for.

### **Language Header**

WFM have support for 12 languages:
`ko`, `ru`, `de`, `fr`, `pt`, `zh-hans`, `zh-hant`, `es`, `it`, `pl`, `uk`, `en`

For response data that includes an `i18n` field, WFM can provide translations in addition to the default English (`en`) translation. 
To request a translation in one of the supported languages, include the `Language: {lang}` header in your API request, where `{lang}` is the code of the desired language.

<aside>
➡️

Default value is `en`

</aside>

Example:
Send request with `Language: ko` 
Get response like:

```json
{
	i18n: {
		en: {...},
		ko: {...}
	}
}
```

### **Platform Header**

WFM is designed to cater to users across various gaming platforms. It currently supports five platforms: `pc`, `ps4`, `xbox`, `switch`, `mobile`

To access content specific to a particular platform, it is necessary to include the `Platform: {platform}` header in your API request. In this header, `{platform}` should be replaced with the code corresponding to the desired platform.

<aside>
➡️

Default value is `pc`

</aside>

### **Crossplay Header**

In addition to platforms, we also have cross-play option.
Players across different platforms can trade if they have cross-play enabled in their game settings.

Here’s how the headers work.
Let’s say you want to retrieve all orders from PC and also include cross-play orders from other platforms. In this case, you can set the headers like this:

```
Platform: pc
Crossplay: true
```

On the other hand, if you only want to get orders from PC and exclude cross-play orders from other platforms, use:

```
Platform: pc
Crossplay: false
```

<aside>
➡️

Default value is `true`

</aside>

## Response Structure

**Response body:**

```json
{
	apiVersion: "x.x.x",
	data: payload | null,
	error: payload | null
}
```

Where:

- **`apiVersion`** **string** - is **semVer** compatible version of our API server.
- **`data`** **object | null** - if request was successful there you can locate your response data
- **`error`** **object | null** - if there were any errors, you'll find them here

## Errors

### **Hitting rate limits or other protection from CF**

WFM implements rate limits to ensure fair usage and protect against excessive requests per second (RPS).
Exceeding these limits results in a `429` error code, often accompanied by a CloudFlare challenge page. In some cases, excessive RPS may lead to outright blocking by CloudFlare.

Additionally, if there are too many concurrent connections from a single IP address, a `509` error code is returned.
This is a measure to prevent overloading the server with multiple connections from the same source.

### **Handled errors**

```tsx
{
  apiVersion: "x.x.x",
	data: null,
	error: {
		request: [...],
		inputs: {
		    fieldOne: "",
		    fieldTwo: "",
		    ...
		},
	},
}
```

Where:

- **`request`** **[]string** - general level request error, like `app.errors.unauthorized`, `app.errors.forbidden`, `app.order.error.exceededOrderLimit`, etc
- **`inputs` map[string]string** - input level errors, like form errors, query param errors, multipart/form-data errors.
Example, you are trying to create an order, and put platinum as -1, you will get:
`inputs: {platinum: “app.field.tooSmall”}`

### **Unhandled errors**

Request killed goroutine for some reason, critical error.
You will get `5xx` error without any content.

# Endpoints

<aside>
ℹ️

Authorization Tokens obtained from v1 Api by default have scope: `all`
This means, if you are using v1 auth, you can ignore scope requirements, for now.

But, eventually v1 login endpoint would be removed in favor of OAuth2.0 system.

</aside>

**Here is our advanced endpoint marking system**:

🔜 - In development

🚧 - Unstable or unfinished endpoints, use at your own risk

♿ - Rate limited endpoints, mind you calls lads

🔒 - Require authorization

💔 - Available only for 1st party apps

🌀 - Content depends on defined [platform](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21) and [crossplay](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21) headers

🇬🇧 - Can request additional [translations](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21)

🚫 - Deprecated

## Manifests

Sounds familiar …

### **`GET**  /v2/versions`

This endpoint retrieves the current version number of the server's resources, formatted either as a **semVer** string or as an arbitrary version identifier.
Whenever the server database is updated or new versions of mobile apps are released, the version number for relevant resources is also updated.
Client applications can check this endpoint periodically to fetch the current server version. A discrepancy between the server's version number and the client's indicates that an update has occurred. In such cases, clients should refresh their local data, like re-downloading item lists, to stay synchronized with the server's latest updates.

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: {
	apps: {
		ios: "x.x.x",
		android: "x.x.x",
		minIos: "x.x.x",
		minAndroid: "x.x.x",
	},
	collections: {
		items: "base64",
		rivens: "base64",
		liches: "base64",
		sisters: "base64",
		missions: "base64",
		npcs: "base64",
		locations: "base64",
	},
	updatedAt: "2021-05-21T14:59:02Z",
},
error: null
}
```

### **`GET**  /v2/items` 🇬🇧

Get list of all tradable items

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/item/{slug}` 🇬🇧

`GET  /v2/itemId/{itemId}`

Get full info about one, particular item

**URL parameters**:

🔹 **`slug`** - `slug` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`itemId`** - `id` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Response**:

```json
{
apiVersion: "x.x.x",
data: [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
error: null
}
```

### **`GET**  /v2/item/{slug}/set` 🇬🇧

`GET  /v2/itemId/{itemId}/set`

Retrieve Information on Item Sets
In WFM, items can either be standalone or part of a set. A set is a collection of related items that are often traded together.

1. If the queried item is not part of any set, the response will contain an array with just that one item.
2. If the item is part of a set or is a set itself, the response will include an array of all items within that set. 

**URL parameters**:

🔹 **`slug`** - `slug` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`itemId`** - `id` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Response**:

```json
{
apiVersion: "x.x.x",
data: {
	id: "54a73e65e779893a797fff72",
	items: [
		[Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		[Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		...
	]
}
error: null
}
```

**Response Fields**

- **`id`** **string** - id of an item you requested.
- **`items`** **[]Item** - array of items

### **`GET**  /v2/riven/weapons` 🇬🇧

Get list of all tradable riven items

**URL parameters**:

 `None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Riven](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Riven](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/riven/weapon/{slug}` 🇬🇧

Get full info about one, particular riven item

**URL parameters**:

🔹 **`slug`** - field form [Riven](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) model

**Response**:

```json
{
apiVersion: "x.x.x",
data: [Riven](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
error: null
}
```

### **`GET**  /v2/riven/attributes` 🇬🇧

Get list of all attributes for riven weapons

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[RivenAttribute](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[RivenAttribute](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/lich/weapons` 🇬🇧

Get list of all tradable lich weapons

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[LichWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[LichWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/lich/weapon/{slug}` 🇬🇧

Get full info about one, particular lich weapon

**URL parameters**:

🔹 **`slug`** - field form LichWeapon model

**Response**:

```json
{
apiVersion: "x.x.x",
data: [LichWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
error: null
}
```

### **`GET**  /v2/lich/ephemeras` 🇬🇧

Get list of all tradable lich ephemeras

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[LichEphemera](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[LichEphemera](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/lich/quirks` 🇬🇧

Get list of all tradable lich quirks

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[LichQuirk](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[LichQuirk](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/sister/weapons` 🇬🇧

Get list of all tradable sister weapons

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[SisterWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[SisterWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/sister/weapon/{slug}` 🇬🇧

Get full info about one, particular sister weapon

**URL parameters**:

🔹 **`slug`** - field form SisterWeapon model

**Response**:

```json
{
apiVersion: "x.x.x",
data: [SisterWeapon](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
error: null
}
```

### **`GET**  /v2/sister/ephemeras` 🇬🇧

Get list of all tradable sister ephemera’s

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[SisterEphemera](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[SisterEphemera](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/sister/quirks` 🇬🇧

Get list of all tradable sister quirks

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[SisterQuirk](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[SisterQuirk](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/locations` 🇬🇧

Get list of all locations (~~that are known to WFM~~)

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Location](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Location](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/npcs` 🇬🇧

Get list of all NPC’s (~~that are known to WFM~~)

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Npc](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Npc](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/missions` 🇬🇧

Get list of all Missions (~~that are known to WFM~~)

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Mission](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Mission](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

## Orders

### **`GET**  /v2/orders/recent` 🌀

Get the most recent orders.
500 max, for the last 4 hours, sorted by `createdAt`

Cached, with 1min refresh interval.

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
]
error: null
}
```

### **`GET**  /v2/orders/item/{slug}` 🌀

`GET  /v2/orders/itemId/{itemId}`

Get a list of all orders for an item from users who was online within the last 7 days.

**URL parameters**:

🔹 **`slug`** - `slug` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`itemId`** - `id` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
]
error: null
}
```

### **`GET**  /v2/orders/item/{slug}/top` 🌀

`GET  /v2/orders/itemId/{itemId}/top`

This endpoint is designed to fetch the top 5 buy and top 5 sell orders for a specific item, exclusively from online users. 
Orders are sorted by price.

**URL parameters**:

🔹 **`slug`** - `slug` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`itemId`** - `id` field form [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) and [ItemShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Query parameters** 

 ▸  **`rank` int** - Filters orders by the **exact** rank specified.
To retrieve all orders for “Arcane Energize” with a rank of **4**, include `rank=4` in the query.
This parameter is ignored if the `rankLt` parameter is provided.
Accepts value between 0 and Max possible rank of an [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`rankLt` int** - Filters orders with a rank **less than** the specified value.

To retrieve all orders for “Arcane Energize” with a rank less than the maximum possible value of **5**, include `rankLt=5` in the query.
If both `rank` and `rankLt` are provided, `rankLt` takes precedence.
Accepts value between 1 and Max possible rank of an [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`charges` int** - Filters orders by the **exact** number of charges left.

To retrieve all orders for “Lohk” with exactly 2 charges left, include `charges=2` in the query.
This parameter is ignored if the `chargesLt` parameter is provided.
Accepts value between 0 and maximum possible charges for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`chargesLt` int** - Filters orders where the number of charges left **is less than** the specified value.
To retrieve all orders for “Lohk” with a charges less than the maximum possible value of **3**, include `chargesLt=3` in the query.
If both `charges` and `chargesLt` are provided, `chargesLt` takes precedence.
Accepts value between 1 and maximum possible charges for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`amberStars` int** - Filters orders by the **exact** number of amber stars.

To retrieve all orders for “Ayatan Anasa Sculpture” with exactly 1 amber star, include `amberStars=1` in the query.
This parameter is ignored if the `amberStarsLt` parameter is provided.
Accepts value between 0 and maximum possible amount of amber stars for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`amberStarsLt` int** - Filters orders where the number of amber stars **is less than** the specified value.
To retrieve all orders for “Ayatan Anasa Sculpture” with an amber stars less than the maximum possible value of 2, include `amberStarsLt=2` in the query.
If both `amberStars` and `amberStarsLt` are provided, `amberStarsLt` takes precedence.
Accepts value between 1 and maximum possible amount of amber stars for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`cyanStars` int** - Filters orders by the **exact** number of cyan stars.

To retrieve all orders for “Ayatan Anasa Sculpture” with exactly 1 cyan star, include `cyanStars=1` in the query.
This parameter is ignored if the `cyanStarsLt` parameter is provided.
Accepts value between 0 and maximum possible amount of cyan stars for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`cyanStarsLt` int** - Filters orders where the number of cyan stars **is less than** the specified value.
To retrieve all orders for “Ayatan Anasa Sculpture” with an cyan stars less than the maximum possible value of 2, include `cyanStarsLt=2` in the query.
If both `cyanStars` and `cyanStarsLt` are provided, `cyanStarsLt` takes precedence.
Accepts value between 1 and maximum possible amount of cyan stars for the [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

 ▸  **`subtype` string** - controls the filtering of orders based on item `subtype` field.

To retrieve all orders for crafted “Ambassador Receiver”, include `subtype=crafted` in the query.
Accepts any valid subtype form an [Item](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21).

**Response**:

```json
{
apiVersion: "x.x.x",
data: {
	buy: [
		[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		...
	],
	sell: [
		[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		[OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		...
	]
},
error: null
}
```

### **`GET**  /v2/orders/user/{slug}`

`GET /v2/orders/userId/{userId}`

Getting public orders from specified user.

**URL parameters**:

🔹 **`userId`** - `id` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`slug`** - `slug` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/orders/my` 🔒

This endpoint retrieves all orders associated with the currently authenticated user.

**URL parameters**:

`None`

**Response**:

```json
{
apiVersion: "x.x.x",
data: [
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	[Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	...
],
error: null
}
```

### **`GET**  /v2/order/{id}`

**URL parameters**:

🔹 **`id`** - id of an Order

**Response**:

```json
{
	apiVersion: "x.x.x",
	data: [OrderWithUser](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`POST**  /v2/order` 🔒

Create a new order

**Request body**

```json
{
	"itemId": "54aae292e7798909064f1575",
	"type": "sell",
	"platinum": 38,
	"quantity": 12,
	"visible": true,
	"perTrade": 6,
	"rank": 5,
	"charges": 3,
	"subtype": "blueprint",
	"amberStars": 3,
	"cyanStars": 3,
}
```

**Request Fields**

- **`itemId`** **string** - The ID of an item. You can obtain it from [here](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21)
- **`type`** ”**sell**” | “**buy**” - The type of order
- **`platinum`** **int** - The price of the item
- **`quantity`** **int** - Your stock, representing how many you have and can sell or buy
- **`visible`** **bool** - Determines if the order should be visible or hidden
- **`perTrade`** **int** - The minimum number of items required per transaction or trade
- **`rank`** **int** - The rank of the item, such as a mod rank
- **`charges`** **int** - The number of charges remaining (e.g., for parazon mods)
- **`subtype`** **string** - The item's subtype. Refer to the Item model for the possible subtypes an item may have (if applicable)
- **`amberStars`** **int** - The number of installed amber stars
- **`cyanStars`** **int** - The number of installed cyan stars

**Response**:

```json
{
	apiVersion: "x.x.x",
	data: [Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`PATCH**  /v2/order/{id}` 🔒

Patch already existing order.

**URL parameters**:

🔹**`Id`** - id field form Order model

**Request body**

```json
{
  "platinum": 10,
  "quantity": 12,
  "perTrade": 3,
  "rank": 1,
  "charges": 2,
  "amberStars": 2,
  "cyanStars": 2,
  "subtype": "blueprint",
  "visible": false
}
```

**Request Fields**

- **`platinum`** **int** - The price of the item
- **`quantity`** **int** - Your stock, representing how many you have and can sell or buy
- **`visible`** **bool** - Determines if the order should be visible or hidden
- **`perTrade`** **int** - The minimum number of items required per transaction or trade
- **`rank` int** - The rank of the item, such as a mod rank
- **`charges` int** - The number of charges remaining (e.g., for parazon mods)
- **`subtype` string** - The item's subtype. Refer to the Item model for the possible subtypes an item may have (if applicable)
- **`amberStars` int** - The number of installed amber stars
- **`cyanStars` int** - The number of installed cyan stars

**Response**

```json
{
	apiVersion: "x.x.x",
	data: Order,
	error: null
}
```

### **`DELETE**  /v2/order/{id}` 🔒

**URL parameters**:

🔹 **`Id`** - id field form Order model

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [Order](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`POST**  /v2/order/{id}/close` 🔒

Close a portion or all of an existing order.

Allows you to close part of an open order by specifying a quantity to reduce.

For example, if your order was initially created with a quantity of 20, and you send a request to close 8 units, the remaining quantity will be 12.

If you close the entire remaining quantity, the order will be considered fully closed and removed.

**URL parameters**:

🔹 **`Id`** - id field form Order model

**Request body**

```json
{
	"quantity": 12,
}
```

**Request Fields**

- **`quantity`** **int** - The number of units to close (subtract from the order's current quantity).

**Response**:

```json
{
	apiVersion: "x.x.x",
	data: [Transaction](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`PATCH**  /v2/orders/group/{id}` 🔒

Update group of orders

**URL parameters**:

🔹 **`Id`** - Group Id, for now only `all` and `ungrouped` are available

**Request body**

```json
{
	"visible": false,
	"type": "sell",
}
```

**Request Fields**

- **`visible`** **bool** - visibility state of all orders withing a group
- **`type`** **”sell” | “buy”** - target only specific type of orders within a group

**Response**:

```json
{
	apiVersion: "x.x.x",
	data: {
		updated: 13,
	}
	error: null
}
```

**Response Fields**

- **`updated`** **int** - How many orders were updated

## Groups

Endpoints to manage order and contract groups.

Currently, only two virtual groups are available for use with any endpoint that requires a `groupId`:

- `all` - includes all orders \ contracts, whether grouped or ungrouped.
- `ungrouped` - includes only orders \ contracts that are not assigned to any group.

<aside>
ℹ️

Groups management endpoints will be published later (create, update, delete, etc)

</aside>

## Users

### **`GET**  /v2/me` 🔒

Getting information about current authenticated user.

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [UserPrivate](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`PATCH**  /v2/me` 🔒

Updating your own user record

**Request body**

```json
{
	"about": "something",
	"platform": "mobile",
	"crossplay": true,
	"locale": "pt",
	"theme": "light",
	"syncLocale": true,
	"syncTheme": true,
}
```

**Request Fields**

- **`about` string** - profile description
- **`platform` one of** [**platforms**](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21) - main platform you are playing on
- **`crossplay` bool** - is crossplay enabled for your warframe account
- **`locale` one of** [**languages**](https://www.notion.so/WFM-Api-v2-Documentation-5d987e4aa2f74b55a80db1a09932459d?pvs=21) - UI locale and preferable communication language
- **`theme` ”light” | “dark” | “system”** - UI theme
- **`syncLocale` bool** - should we sync locale across devices
- **`syncTheme` bool** - should we sync theme across devices

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [UserPrivate](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`POST**  /v2/me/avatar` 🔒

Update your avatar.
The uploaded image will be resized to **256×256** pixels, focusing on the center of the image.

**Request**

Content-Type: `multipart/form-data`

```go
form-data; name="avatar"; filename="whatever.png"
```

**Request Fields**

- `avatar` **file** - 	Image file to upload as the new avatar.

**Constraints**

- Accepts image formats: `.png, .jpg, .jpeg, .webp, .gif, .bmp, .avif`
- Maximum upload size is **5mb**

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [UserPrivate](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`POST**  /v2/me/background` 🔒

<aside>
ℹ️

Requires user to have at least a silver subscription tier

</aside>

Update your profile background.
The uploaded image will be resized to **1920×820** pixels, focusing on the center of the image.

**Request**

Content-Type: `multipart/form-data`

```go
form-data; name="background"; filename="whatever.png"
```

**Request Fields**

- `background` **file** - 	Image file to upload as the new background.

**Constraints**

- Accepts image formats: `.png, .jpg, .jpeg, .webp, .gif, .bmp, .avif`
- Maximum upload size is **8mb**

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [UserPrivate](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

### **`GET**  /v2/user/{slug}`

`GET /v2/userId/{userId}`

Getting information about particular user

**URL parameters**:

🔹 **`userId`** - `id` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`slug`** - `slug` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
	error: null
}
```

## Achievements

### **`GET**  /v2/achievements`

Get list of all available achievements, except secret ones.

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [
		[Achievement](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		[Achievement](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		...
	],
	error: null
}
```

### **`GET**  /v2/achievements/user/{slug}`

`GET /v2/achievements/userId/{userId}`

Get a list of all user achievements

**URL parameters**:

🔹 **`userId`** - `id` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

🔹 **`slug`** - `slug` field form [User](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) or [UserShort](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21) models

**Query parameters** 

 ▸  **`featured` bool** - Return only `featured: true` achievements.

**Response**

```json
{
	apiVersion: "x.x.x",
	data: [
		[Achievement](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		[Achievement](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21),
		...
	],
	error: null
}
```

## **Authentication**

### **`POST**  /auth/signin` 💔

<aside>
⚠️

Login, first party applications only

</aside>

- Wfm Frontend
- Ios app
- Android app

**Request body**

```json
{
	"email": "whatever@gmail.com",
	"password": "12345", 
	"clientId": "wfm-0000",
	"deviceId": "random uniq id",
	"deviceName": "My cool Nokia 3310"
}
```

**Request Fields**

- **`email` string** - user email
- **`password` string** - user password
- **`clientId` string** - Id of registered [Client](https://www.notion.so/OAuth-2-0-04e1e72398db4cf8ae1b7b1bae4abcc1?pvs=21), first party apps clients ids are:
    - `redacted-0000` frontend
    - `redacted-0001` android app
    - `redacted-0002` ios app
- **`deviceId` string** - Id of device, used to identify this specific device and tie all sessions to it.
- **`deviceName` string** - Name of the device, human readable.

**Headers**

This should be set:

🟠 **`X-Firebase-AppCheck`** **string** - AppCheck verification token

**Response**

```json
{
	"apiVersion": "x.x.x",
	"data":{
		"accessToken": "...",
		"refreshToken": "...",
		"tokenType": "Bearer", 
		"expiresIn": 12345
	},
	"error": null
}
```

### `POST  /auth/signup` 💔

<aside>
⚠️

User registration, first party applications only

</aside>

- WFM Frontend
- Ios app
- Android app

**Request body**

```json
{
	"email": "whatever@gmail.com",
	"password": "12345",
	"clientId": "wfm-0000",
	"deviceId": "random uniq id",
	"deviceName": "My Nokia 3310"
	"platform": "pc",
	"locale": "ko"
}
```

**Request Fields**

- **`email` string** - user email
- **`password` string** - user password
- **`clientId` string** - Id of registered [Client](https://www.notion.so/OAuth-2-0-04e1e72398db4cf8ae1b7b1bae4abcc1?pvs=21), first party apps clients ids are:
    - `redacted-0000` frontend
    - `redacted-0001` android app
    - `redacted-0002` ios app
- **`deviceId` string** - Id of device, could be random generated string, used to identify this specific device and tie all sessions to it.
- **`deviceName` string** - Name of the device, human readable.
- **`platform` string** - platform user are playing on, one of: [Platform](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21)
- **`language` string** - preferred communication language in terms of WFM, should be one of: [Language](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21). You may pick value from something like primary system language of the user device.

**Headers**

This should be set:

🟠 **`X-Firebase-AppCheck`** **string** - AppCheck verification token

**Response**

```json
{
	"apiVersion": "x.x.x",
	"data":{
		"accessToken": "...",
		"refreshToken": "...",
		"tokenType": "Bearer", 
		"expiresIn": 12345
	},
	"error": null
}
```

### `POST  /auth/refresh` 🔒

Refresh all session tokens

**Request body**

```json
{
	"grantType": "refresh_token",
	"clientId": "wfm-0000",
	"deviceId": "BASD001-KSADFDG"
	"refreshToken": "JwtRefreshToken"
}
```

**Request Fields**

- **`grantType` string** - should be `refresh_token`
- **`clientId` string** - Id of registered [Client](https://www.notion.so/OAuth-2-0-04e1e72398db4cf8ae1b7b1bae4abcc1?pvs=21)
- **`deviceId` string** - Id of device, used to identify this specific device and tie all sessions to it.
- **`refreshToken` string** - usually almost expired refresh token

**Headers**

<aside>
🔻

Only first party apps should set this header

</aside>

🟠 **`X-Firebase-AppCheck`** **string** - AppCheck verification token

**Response**

```json
{
	"apiVersion": "x.x.x",
	"data":{
		"accessToken": "...",
		"refreshToken": "...",
		"tokenType": "Bearer", 
		"expiresIn": 12345
	},
	"error": null
}
```

### `POST  /auth/signout` 🔒

Terminate current session.
Refresh and access tokens will become unusable.

**Request body**

```json
{}
```

**Headers**

🟠 **`Authorization` string** - JWT access token in format: **`Bearer** YourJWTAccessToken`

- Example
    
    **`Authorization: Bearer** iOiJ2SVlqdUdkclliT.lhsOU9xYk42RFhHWWExOWN3RmpBNSIsImNzcmZfdG9rZW4iOiJmOWU1YmM.2NWY5NDA1YmU1NzU3`
    

**Response**

`empty` (no body), status code: `200`

## Dashboard

### **`GET**  /v2/dashboard/showcase` 🇬🇧

Mobile app main screen dashboard with featured items.

**Response**:

```json
{
apiVersion: "x.x.x",
data: {
	[DashboardShowcase](https://www.notion.so/Data-Models-65e9ab01868c4dcca6ba499e68a04ac9?pvs=21)
},
error: null
}
```

## OAuth

 `/oauth/authorize` - authorize request by 3rd party account
 `/oauth/token` - exchange code for access token, with PKCE
 `/oauth/revoke` - revoke access token