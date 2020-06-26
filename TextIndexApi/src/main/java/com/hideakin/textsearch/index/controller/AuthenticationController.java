package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.AuthenticateResponse;
import com.hideakin.textsearch.index.service.UserService;

@RestController
public class AuthenticationController {

	@Autowired
	private UserService userService;

	@RequestMapping(value="/v1/authentication",method=RequestMethod.GET)
	public AuthenticateResponse getApiKey(
			@RequestHeader("authorization") String authorizationHeader) {
		return userService.verifyUsernamePassword(authorizationHeader.trim());
	}

}
