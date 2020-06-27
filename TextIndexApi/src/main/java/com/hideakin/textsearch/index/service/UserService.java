package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.data.VerifyApiKeyResult;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;

public interface UserService {

	AuthenticateResult authenticate(String username, String password);
	VerifyApiKeyResult verifyApiKey(String key, String role);
	UserInfo[] getUsers();
	UserInfo getUser(String username);
	void createUser(String username, String password, String roles);
	boolean updateUser(String username, String password, String roles);
	boolean deleteUser(String username);

}