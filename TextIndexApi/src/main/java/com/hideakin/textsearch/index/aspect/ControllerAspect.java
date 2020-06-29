package com.hideakin.textsearch.index.aspect;

import java.time.ZonedDateTime;

import javax.servlet.http.HttpServletRequest;

import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Before;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpMethod;
import org.springframework.stereotype.Component;
import com.hideakin.textsearch.index.data.AuthenticateError;
import com.hideakin.textsearch.index.exception.ForbiddenException;
import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.exception.UnauthorizedException;
import com.hideakin.textsearch.index.model.BearerToken;
import com.hideakin.textsearch.index.model.MethodRoleCollection;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.service.PreferenceService;
import com.hideakin.textsearch.index.service.UserService;
import com.hideakin.textsearch.index.utility.RolesIntersection;

@Component
@Aspect
public class ControllerAspect {

	private static final Logger logger = LoggerFactory.getLogger(ControllerAspect.class);

	@Autowired
	private UserService userService;

	@Autowired
	private PreferenceService preferenceService;

	private MethodRoleCollection administratorOnly;
	private MethodRoleCollection userReadOnly;
	private MethodRoleCollection maintainerCanChange;
	
	public ControllerAspect() {
		final String administrator = "administrator";
		final String user = "user";
		final String maintainer = "maintainer";
		administratorOnly = new MethodRoleCollection();
		administratorOnly
			.add(HttpMethod.GET, administrator)
			.add(HttpMethod.POST, administrator)
			.add(HttpMethod.PUT, administrator)
			.add(HttpMethod.DELETE, administrator);
		userReadOnly = new MethodRoleCollection();
		userReadOnly
			.add(HttpMethod.GET, user, maintainer, administrator)
			.add(HttpMethod.POST, administrator)
			.add(HttpMethod.PUT, administrator)
			.add(HttpMethod.DELETE, administrator);
		maintainerCanChange = new MethodRoleCollection();
		maintainerCanChange
			.add(HttpMethod.GET, user, maintainer, administrator)
			.add(HttpMethod.POST, maintainer, administrator)
			.add(HttpMethod.PUT, maintainer, administrator)
			.add(HttpMethod.DELETE, maintainer, administrator);
	}

	@Before("within(com.hideakin.textsearch.index.controller.MaintenanceController)")
	public void beforeMaintenanceController() {
	    verifyApiKey(userReadOnly);
	}
	
	@Before("within(com.hideakin.textsearch.index.controller.UserController)")
	public void beforeUserController() {
		verifyServiceAvailability();
	    verifyApiKey(administratorOnly);
	}
	
	@Before("within(com.hideakin.textsearch.index.controller.FileController)")
	public void beforeFileController() {
		verifyServiceAvailability();
		verifyApiKey(maintainerCanChange);
	}

	@Before("within(com.hideakin.textsearch.index.controller.FileGroupController)")
	public void beforeFileGroupController() {
		verifyServiceAvailability();
		verifyApiKey(maintainerCanChange);
	}

	@Before("within(com.hideakin.textsearch.index.controller.IndexController)")
	public void beforeIndexController() {
		verifyServiceAvailability();
		verifyApiKey(maintainerCanChange);
	}

	@Before("within(com.hideakin.textsearch.index.controller.PreferenceController)")
	public void beforePreferenceController() {
		verifyServiceAvailability();
		verifyApiKey(maintainerCanChange);
	}

	private void verifyServiceAvailability() {
		if (preferenceService.isServiceUnavailable()) {
			logger.warn("API is under maintenance.");
			throw new ServiceUnavailableException();
		}
	}

	private void verifyApiKey(MethodRoleCollection mrc) {
		HttpServletRequest request = RequestContext.getRequest();
	    String header = request.getHeader("authorization");
	    if (header == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No authorization header.");
	    }
	    BearerToken bt = BearerToken.parse(header.trim());
	    if (bt == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Malformed authorization header.");
	    }
	    UserInfo userInfo = userService.getUserByApiKey(bt.getToken());
	    if (userInfo == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Invalid API key.");	    	
	    } else if (userInfo.getExpiry().isBefore(ZonedDateTime.now())) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Expired API key.");	    	
	    }
	    String[] roles = mrc.find(request.getMethod());
	    if (roles != null && !RolesIntersection.Exists(roles, userInfo.getRoles())) {
	    	throw new ForbiddenException();
	    }
	    RequestContext.setUserInfo(userInfo);
	}

}
