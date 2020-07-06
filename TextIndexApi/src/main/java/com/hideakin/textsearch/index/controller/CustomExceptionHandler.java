package com.hideakin.textsearch.index.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestControllerAdvice;
import org.springframework.web.context.request.WebRequest;
import org.springframework.web.servlet.mvc.method.annotation.ResponseEntityExceptionHandler;

import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.exception.ForbiddenException;
import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.exception.UnauthorizedException;
import com.hideakin.textsearch.index.model.AuthenticateErrorResponse;
import com.hideakin.textsearch.index.model.ErrorResponse;

@RestControllerAdvice
public class CustomExceptionHandler extends ResponseEntityExceptionHandler {

	@ExceptionHandler(InvalidParameterException.class)
	public ResponseEntity<?> onBadParameter(InvalidParameterException ex, WebRequest request) {
		ErrorResponse body = new ErrorResponse("invalid_parameter", ex.getMessage());
		return new ResponseEntity<>(body, HttpStatus.BAD_REQUEST);
	}

	@ExceptionHandler(UnauthorizedException.class)
	public ResponseEntity<?> onUnauthorized(UnauthorizedException ex, WebRequest request) {
		AuthenticateErrorResponse body = new AuthenticateErrorResponse(ex.getError(), ex.getErrorDescription());
		return new ResponseEntity<>(body, HttpStatus.UNAUTHORIZED);
	}

	@ExceptionHandler(ForbiddenException.class)
	public ResponseEntity<?> onForbidden(ForbiddenException ex, WebRequest request) {
		ErrorResponse body = new ErrorResponse("access_denied", ex.getMessage());
		return new ResponseEntity<>(body, HttpStatus.FORBIDDEN);
	}

	@ResponseStatus(HttpStatus.SERVICE_UNAVAILABLE)
	@ExceptionHandler(ServiceUnavailableException.class)
	public void onServiceUnavailable() {
		// nothing needs to be done
	}

}
