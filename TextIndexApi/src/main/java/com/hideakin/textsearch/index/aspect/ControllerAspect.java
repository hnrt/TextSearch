package com.hideakin.textsearch.index.aspect;

import javax.servlet.http.HttpServletRequest;

import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Before;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.web.context.request.RequestContextHolder;
import org.springframework.web.context.request.ServletRequestAttributes;

import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.service.PreferenceService;
import com.hideakin.textsearch.index.service.UserService;

@Component
@Aspect
public class ControllerAspect {

	private static final Logger logger = LoggerFactory.getLogger(ControllerAspect.class);

	@Autowired
	UserService userService;

	@Autowired
	PreferenceService preferenceService;

	@Before("within(com.hideakin.textsearch.index.controller.MaintenanceController)")
	public void beforeMaintenanceController() {
	    verifyApiKey("administrator");
	}
	
	@Before("within(com.hideakin.textsearch.index.controller.FileController)")
	public void beforeFileController() {
		verifyServiceAvailability();
		verifyApiKey();
	}

	@Before("within(com.hideakin.textsearch.index.controller.FileGroupController)")
	public void beforeFileGroupController() {
		verifyServiceAvailability();
		verifyApiKey();
	}

	@Before("within(com.hideakin.textsearch.index.controller.IndexController)")
	public void beforeIndexController() {
		verifyServiceAvailability();
		verifyApiKey();
	}

	@Before("within(com.hideakin.textsearch.index.controller.PreferenceController)")
	public void beforePreferenceController() {
		verifyServiceAvailability();
		verifyApiKey();
	}

	private void verifyServiceAvailability() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

	private void verifyApiKey() {
		HttpServletRequest request = ((ServletRequestAttributes)RequestContextHolder.getRequestAttributes()).getRequest();
	    String header = request.getHeader("authorization");
	    userService.verifyApiKey(header);
	}

	private void verifyApiKey(String requiredRole) {
		HttpServletRequest request = ((ServletRequestAttributes)RequestContextHolder.getRequestAttributes()).getRequest();
	    String header = request.getHeader("authorization");
	    userService.verifyApiKey(header, requiredRole);
	}

}
