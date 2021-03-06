package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.service.PreferenceService;

@RestController
public class MaintenanceController {

	@Autowired
	PreferenceService preferenceService;

	@RequestMapping(value="/v1/maintenance",method=RequestMethod.GET)
	public String getMaintenanceMode() {
		return preferenceService.isServiceUnavailable() ? "true" : "false";
	}

	@RequestMapping(value="/v1/maintenance",method=RequestMethod.POST)
	public String enterMaintenanceMode() {
		preferenceService.setServiceAvailability(false);
		return "OK";
	}

	@RequestMapping(value="/v1/maintenance",method=RequestMethod.DELETE)
	public String leaveMaintenanceMode() {
		preferenceService.setServiceAvailability(true);
		return "OK";
	}

}
