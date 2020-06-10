package com.hideakin.textsearch.index.aspect;

import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Before;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.service.PreferenceService;

@Component
@Aspect
public class ControllerAspect {

	private static final Logger logger = LoggerFactory.getLogger(ControllerAspect.class);

	@Autowired
	PreferenceService preferenceService;

	@Before("within(com.hideakin.textsearch.index.controller.FileController)")
	public void beforeFileController() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.FileGroupController)")
	public void beforeFileGroupController() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.IndexController)")
	public void beforeIndexController() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.PreferenceController)")
	public void beforePreferenceController() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

}
