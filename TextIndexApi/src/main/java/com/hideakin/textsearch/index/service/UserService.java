package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.data.VerifyApiKeyResult;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;

public interface UserService {

	AuthenticateResult authenticate(String username, String password);
	VerifyApiKeyResult verifyApiKey(String key, String[] roles);
	UserInfo[] getUsers();
	UserInfo getUser(String username);
	UserInfo getUserByApiKey(String apiKey);
	UserInfo createUser(String username, String password, String[] roles);
	UserInfo updateUser(String username, String password, String[] roles);
	UserInfo deleteUser(String username);

}
