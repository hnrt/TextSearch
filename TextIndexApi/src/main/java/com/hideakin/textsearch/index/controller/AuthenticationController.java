package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.data.AuthenticateError;
import com.hideakin.textsearch.index.exception.UnauthorizedException;
import com.hideakin.textsearch.index.model.AuthenticateRequest;
import com.hideakin.textsearch.index.model.AuthenticateResponse;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.BasicAuthentication;
import com.hideakin.textsearch.index.service.UserService;

@RestController
public class AuthenticationController {

	@Autowired
	private UserService userService;

	@RequestMapping(value="/v1/authentication",method=RequestMethod.GET)
	public AuthenticateResponse authenticateByBasicAuthentication(
			@RequestHeader("authorization") String authorizationHeader) {
		if (authorizationHeader == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No authorization header.");
		}
		BasicAuthentication ba = BasicAuthentication.parse(authorizationHeader.trim());
		if (ba == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Malformed authorization header.");
		}
		return authenticate(ba.getUsername(), ba.getPassword());
	}

	@RequestMapping(value="/v1/authentication",method=RequestMethod.POST,consumes=MediaType.APPLICATION_JSON_VALUE)
	public AuthenticateResponse authenticateByJson(
			@RequestBody AuthenticateRequest req) {
		return authenticate(req.getUsername(), req.getPassword());
	}

	@RequestMapping(value="/v1/authentication",method=RequestMethod.POST,consumes=MediaType.APPLICATION_FORM_URLENCODED_VALUE)
	public AuthenticateResponse authenticateByFormUrlencoded(
			@RequestParam MultiValueMap<String,String> paramMap) {
		return authenticate(paramMap.getFirst("username"), paramMap.getFirst("password"));
	}

	private AuthenticateResponse authenticate(String username, String password) {
		if (username == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No username.");
		}
		if (password == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No password.");
		}
		AuthenticateResult ar = userService.authenticate(username, password);
		if (ar == null) {
			try {
				Thread.sleep(3000); // to prevent frequent attempts
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Invalid credentials.");
		}
		return new AuthenticateResponse(ar.getAccessToken(), "bearer", ar.getExpiresIn());
	}

}
