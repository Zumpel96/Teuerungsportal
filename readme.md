<h1 align="center"> Teuerungsportal </h1>
<h3 align="center"> https://teuerungsportal.at</h3>


![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/Zumpel96/Teuerungsportal)
![GitHub all releases](https://img.shields.io/github/downloads/Zumpel96/Teuerungsportal/total)
![GitHub contributors](https://img.shields.io/github/contributors/Zumpel96/Teuerungsportal)
![GitHub last commit](https://img.shields.io/github/last-commit/Zumpel96/Teuerungsportal)
![GitHub issues](https://img.shields.io/github/issues-raw/Zumpel96/Teuerungsportal)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Zumpel96/Teuerungsportal)
![GitHub language count](https://img.shields.io/github/languages/count/Zumpel96/Teuerungsportal)
 
## Description
The "Teuerungsportal" (Translated: inflation portal) is a portal to keep track of the inflation in austrian supermarkets and also to compare prices.

## Features
- compare prices
- track inflation
- search for products
- products are catigorized

## Important
Unfortunately, it is known that the performance of this site is not the best. The reason for this is that the infrastructure, in order to scale well, is relatively expensive. If you like the project and would like to support it directly, you are welcome to leave a small donation on PayPal.

[Donate here](https://www.paypal.com/paypalme/BRuckenstuhl)

## API

### Categories

#### Get Categories `https://api.teuerungsportal.at/categories`
Returns all categories and the corresponding child categories as sub-elements.

#### Get Categories Ungrouped `https://api.teuerungsportal.at/categories/ungrouped`
Returns all categories without the grouping of the sub-elements.

#### Get Category `https://api.teuerungsportal.at/categories/{categoryName}`
Returns a categories meta data by its name.

#### Get Category Price Changes `https://api.teuerungsportal.at/categories/{categoryId}/prices`
Returns all price changes of a category (excluding new entries).

#### Get Category Products `https://api.teuerungsportal.at/categories/{categoryId}/products/{page}`
Returns a paginated result of all products of a category. Only 25 Products are loaded at once.

#### Get Category Products Pages `https://api.teuerungsportal.at/categories/{categoryId}/products/`
Returns the number of pages for the products pagination.

### Products

#### Get Product `https://api.teuerungsportal.at/products/{articleNumber}/store/{storeName}`
Returns a products meta data by its article number and the store name.

#### Get Product Price Changes `https://api.teuerungsportal.at/products/{productId}/prices/{page}`
Returns a paginated result of all price changes of a category. Only 25 Products are loaded at once.

#### Get Product Price Changes Pages `https://api.teuerungsportal.at/products/{productId}/prices`
Returns the number of pages for the price pagination.

#### Get Product Search `https://api.teuerungsportal.at/products/search/{searchString}/{page}`
Returns a paginated result of all products who fit the search criteria. Only 25 Products are loaded at once.

#### Get Product Search Pages `https://api.teuerungsportal.at/products/search/{searchString}`
Returns the number of pages for the search pagination.

#### Get Products Without Category `https://api.teuerungsportal.at/products/noCategory/{page}`
Returns a paginated result of all products without a category. Only 25 Products are loaded at once.

#### Get Product Without Category Pages `https://api.teuerungsportal.at/products/noCategory`
Returns the number of pages for the products without category pagination.

#### Post Update Product Category `https://api.teuerungsportal.at/products/{productId}/category/{categoryId}`
Updates the category of a product, if there is currently no set category.

### Stores

#### Get Stores `https://api.teuerungsportal.at/stores`
Returns all stores.

#### Get Store `https://api.teuerungsportal.at/stores/{storeName}`
Returns a stores meta data by its name.

#### Get Store Price Changes `https://api.teuerungsportal.at/stores/{storeId}/prices`
Returns all price changes of a store (excluding new entries).

#### Get Store Products `https://api.teuerungsportal.at/stores/{storeId}/products/{page}`
Returns a paginated result of all products of a store. Only 25 Products are loaded at once.

#### Get Store Products Pages `https://api.teuerungsportal.at/stores/{storeId}/products/`
Returns the number of pages for the products pagination.

### General

#### Get Announcement `https://api.teuerungsportal.at/announcement`
Returns the current announcement. If there is none, it returns empty strings for the content fields.

#### Get Donators `https://api.teuerungsportal.at/donators`
Returns all the donators.

#### Get Recent Price Changes `https://api.teuerungsportal.at/prices`
Returns all the price changes of the last 30 days.

