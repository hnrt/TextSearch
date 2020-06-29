package com.hideakin.textsearch.index.aspect;

import javax.servlet.http.HttpServletRequest;
import org.springframework.web.context.request.RequestAttributes;
import org.springframework.web.context.request.RequestContextHolder;
import org.springframework.web.context.request.ServletRequestAttributes;

import com.hideakin.textsearch.index.model.UserInfo;

public class RequestContext {

	public static HttpServletRequest getRequest() {
		return ((ServletRequestAttributes)RequestContextHolder.getRequestAttributes()).getRequest();
	}

	private static final String USERINFO_ATTRNAME = "TextIndexApiUserInfo";

	public static UserInfo getUserInfo() {
		return (UserInfo)((ServletRequestAttributes)RequestContextHolder.getRequestAttributes()).getAttribute(USERINFO_ATTRNAME, RequestAttributes.SCOPE_REQUEST);
	}

	public static void setUserInfo(UserInfo value) {
		((ServletRequestAttributes)RequestContextHolder.getRequestAttributes()).setAttribute(USERINFO_ATTRNAME, value, RequestAttributes.SCOPE_REQUEST);
	}

}
