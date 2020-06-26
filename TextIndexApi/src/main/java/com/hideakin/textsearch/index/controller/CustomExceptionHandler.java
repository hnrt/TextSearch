package com.hideakin.textsearch.index.controller;

import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestControllerAdvice;
import org.springframework.web.context.request.WebRequest;
import org.springframework.web.servlet.mvc.method.annotation.ResponseEntityExceptionHandler;

import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.exception.UnauthorizedException;
import com.hideakin.textsearch.index.model.AuthenticateErrorResponse;

@RestControllerAdvice
public class CustomExceptionHandler extends ResponseEntityExceptionHandler {

	@ResponseStatus(HttpStatus.SERVICE_UNAVAILABLE)
	@ExceptionHandler(ServiceUnavailableException.class)
	public void OnServiceUnavailable() {
		// nothing needs to be done
	}

	@ExceptionHandler(UnauthorizedException.class)
	public ResponseEntity<Object> OnUnauthorized(UnauthorizedException ex, WebRequest request) {
		AuthenticateErrorResponse body = new AuthenticateErrorResponse(ex.getError(), ex.getErrorDescription());
		HttpHeaders headers = new HttpHeaders();
		HttpStatus status = HttpStatus.UNAUTHORIZED;
		return new ResponseEntity<>(body, headers, status);
	}

}
