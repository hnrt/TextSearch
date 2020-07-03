package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;

public interface UserService {

	AuthenticateResult authenticate(String username, String password);
	UserInfo[] getUsers();
	UserInfo getUser(int uid);
	UserInfo getUser(String username);
	UserInfo getUserByAccessToken(String accessToken);
	UserInfo createUser(String username, String password, String[] roles);
	UserInfo updateUser(int uid, String username, String password, String[] roles);
	UserInfo deleteUser(int uid);

}
