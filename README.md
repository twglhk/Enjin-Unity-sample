# Enjin-Unity-sample
Enjin tutorial sample project for Unity developer.
You can test Unity Enjin SDK with it.

### How to guide
##### 1. Log in Enjin App
 First, you need to log in Enjin platform you have made. If you don't have Enjin ID or App ID, sign up Enjin platform and make Enjin project first.  
 If you just try to test Enjin and Unity Enjin SDK, then make a project in Kovan Enjin cloud platform. It doesn't need any ENJ or ETH costs.
 You can find the kovan Enjin cloud at [here](https://kovan.cloud.enjin.io/)

![image](https://user-images.githubusercontent.com/22043884/110468060-1caafc80-811b-11eb-8693-57531a3fb436.png)

 If you make project on Kovan Enjin cloud, then you can find API Credentials from settings tap. 
 App ID and App Secret are first keys of input information of this project. You can log in app with these in Unity client.
 
##### 2. Log in Enjin User
 Next, you need to log in Enjin app with 'User Name'. In this project, you must write user name in string data type (not user ID).
 You can find it at 'Team' tap or using query in GraphQL on Kovan Enjin platform. Also you can create user with Enjin SDK or GraphQL. 
 I'll add this user creation function in Unity as soon as possible.

##### 3. Mutate & Query Identities
 Next, you can mutate(create) or query identities. The identity is belong to user. 
 User can have many identities. Each identity has wallet address and linking code that connects to Enjin wallet using QR scan.
 Query Identites already added in the project, but mutate is not. I'll update as soon as possible.
 

 Updating.. 