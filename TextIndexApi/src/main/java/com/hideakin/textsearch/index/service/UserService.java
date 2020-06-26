package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.AuthenticateResponse;

public interface UserService {

	AuthenticateResponse verifyUsernamePassword(String authorization);
	AuthenticateResponse verifyUsernamePassword(String username, String password);
	void verifyApiKey(String authorization);
	void verifyApiKey(String authorization, String role);

}
