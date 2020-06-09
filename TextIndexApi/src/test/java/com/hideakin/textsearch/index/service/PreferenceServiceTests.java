package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class PreferenceServiceTests {

	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private PreferenceService preferenceService = new PreferenceServiceImpl();

	@Test
	public void getPreference_successful() {
		String name = "quux";
		String value = "fred";
		PreferenceEntity entity = new PreferenceEntity(name, value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		ValueResponse rsp = preferenceService.getPreference(name);
		Assertions.assertEquals(value, rsp.getValue());
	}

	@Test
	public void getPreference_notfound() {
		String name = "quux";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		ValueResponse rsp = preferenceService.getPreference(name);
		Assertions.assertEquals(null, rsp.getValue());
	}

	@Test
	public void updatePreference_successful() {
		UpdatePreferenceRequest req = new UpdatePreferenceRequest("foo", "bar");
		preferenceService.updatePreference(req);
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals(req.getName()) && x.getValue().equals(req.getValue())));
	}

	@Test
	public void deletePreference_successful() {
		PreferenceEntity entity = new PreferenceEntity("quux", "fred");
		when(preferenceRepository.findByName(entity.getName())).thenReturn(entity);
		preferenceService.deletePreference(entity.getName());
		verify(preferenceRepository, times(1)).delete(entity);
	}

	@Test
	public void deletePreference_notfound() {
		String name = "quux";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		preferenceService.deletePreference(name);
		verify(preferenceRepository, times(0)).delete(any(PreferenceEntity.class));
	}
	
	@Test
	public void isServiceUnavailable_true() {
		PreferenceEntity entity = new PreferenceEntity("enabled", "false");
		when(preferenceRepository.findByName(entity.getName())).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(true, ret);
	}
	
	@Test
	public void isServiceUnavailable_false() {
		PreferenceEntity entity = new PreferenceEntity("enabled", "true");
		when(preferenceRepository.findByName(entity.getName())).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}
	
	@Test
	public void isServiceUnavailable_invalid() {
		PreferenceEntity entity = new PreferenceEntity("enabled", "xyzzy");
		when(preferenceRepository.findByName(entity.getName())).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}
	
	@Test
	public void isServiceUnavailable_null() {
		String name = "enabled";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}

	@Test
	public void setServiceAvailability_successful() {
		preferenceService.setServiceAvailability(true);
		verify(preferenceRepository, times(1)).save(any(PreferenceEntity.class));
	}

}
